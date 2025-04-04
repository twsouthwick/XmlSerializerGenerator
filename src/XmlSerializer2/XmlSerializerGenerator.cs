using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace XmlSerializer2;

#pragma warning disable RSEXPERIMENTAL002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

[Generator]
public class XmlSerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            const string Source = """
            namespace System.Xml.Serialization;

            #pragma warning disable CS9113

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
            internal sealed class XmlSerializableAttribute : Attribute
            {
                public XmlSerializableAttribute(Type type)
                {
                    Type = type;
                }

                public Type Type { get; }
            }
            
            [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
            internal sealed class XmlSerializerOverrideAttribute : Attribute
            {
                public XmlSerializerOverrideAttribute(Type type, string name)
                {
                }
            }
            """;

            context.AddSource("XmlSerializableAttribute.g.cs", SourceText.From(Source, Encoding.UTF8));
        });

        var options = context.AnalyzerConfigOptionsProvider.Select((options, token) =>
        {
            if (options.GlobalOptions.TryGetValue("build_property.SgenMethodOverride", out var value))
            {
                return value.Split(';').ToImmutableHashSet();
            }

            return [];
        });

        var overrides = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "System.Xml.Serialization.XmlSerializerOverrideAttribute",
                static (node, token) => true,
                static (context, token) =>
                {
                    var list = new List<string>();

                    foreach (var attr in context.Attributes)
                    {
                        if (attr.ConstructorArguments is [{ Value: INamedTypeSymbol type }, { Value: string name }])
                        {
                            foreach (var member in type.GetMembers(name))
                            {
                                list.Add(member.ToDisplayString());
                            }
                        }
                    }
                    return list;
                });

        var methodsToOverride = options.Combine(overrides.Collect())
            .Select((t, _) =>
            {
                var result = t.Left;

                foreach (var r in t.Right)
                {
                    result = result.Union(r);
                }

                return result;
            });

        var operations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, token) => node is InvocationExpressionSyntax,
            transform: (context, token) =>
            {
                var interceptableLocation = context.SemanticModel.GetInterceptableLocation((InvocationExpressionSyntax)context.Node, token);
                if (interceptableLocation is null)
                {
                    return null; // generator wants to intercept call, but host thinks call is not interceptable
                }

                var systemXmlSerializer = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Xml.Serialization.XmlSerializer");

                if (context.SemanticModel.GetOperation(context.Node, token) is IInvocationOperation operation
                    && operation.ChildOperations is { Count: 1 }
                    && operation.ChildOperations.First() is IArgumentOperation arg
                    && arg.ChildOperations is { Count: 1 }
                    && arg.ChildOperations.First() is ITypeOfOperation { TypeOperand: { } type }
                    && SymbolEqualityComparer.Default.Equals(systemXmlSerializer, operation.Type))
                {
                    return new InterceptInfo(context.Node.GetLocation(), interceptableLocation, operation.TargetMethod.ToDisplayString(), type.ToDisplayString());
                }

                return null;
            })
            .Combine(methodsToOverride)
            .Where(values => values.Left is { MethodName: { } n } && values.Right.Contains(n))
            .Select((v, _) => (Info)v.Left!);

        // Register a syntax provider to find object creation expressions for XmlSerializer
        var xmlSerializerContext = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "System.Xml.Serialization.XmlSerializableAttribute",
                (node, token) => true,
                (context, token) =>
                {
                    var location = context.TargetNode.GetLocation();
                    var symbol = context.SemanticModel.GetDeclaredSymbol(context.TargetNode, token);

                    if (symbol is INamedTypeSymbol typeSymbol)
                    {
                        var types = GetAttributeNames(typeSymbol);
                        var ns = GetNamespaceName(typeSymbol);

                        return (Info)new CreateClassInfo(location, ns, GetContainingClasses(typeSymbol), [.. types]);
                    }

                    static string? GetNamespaceName(ISymbol symbol)
                        => symbol.ContainingNamespace is { IsGlobalNamespace: false } n
                            ? n.ToDisplayString() : null;

                    static IEnumerable<string> GetAttributeNames(ISymbol symbol) => symbol
                        .GetAttributes()
                        .Where(a => a.AttributeClass!.Name.Equals("XmlSerializableAttribute"))
                        .Select(a => a.ConstructorArguments is [{ Value: INamedTypeSymbol type }] ? type : null)
                        .OfType<INamedTypeSymbol>()
                        .Select(t => t.ToDisplayString());

                    return (Info)new ErrorInfo(location, "XmlSerializable can only be used on a partial class or a static method that takes a Type and returns an XmlSerializer.");

                    static ImmutableArray<string> GetContainingClasses(INamedTypeSymbol c)
                    {
                        var containingClasses = ImmutableArray.CreateBuilder<string>();

                        while (c is not null)
                        {
                            containingClasses.Add(c.Name);
                            c = c.ContainingType;
                        }

                        return containingClasses.ToImmutable();
                    }
                });

        var infos = operations.Collect()
            .Combine(xmlSerializerContext.Collect())
            .SelectMany((s, token) => s.Left.Concat(s.Right))
            .Collect();

        var sourceGenerator = context.CompilationProvider
            .Combine(infos)
            .SelectMany((ctx, token) =>
            {
                var result = new List<Result>();
                var intercepts = new List<InterceptInfo>();
                var classes = new List<CreateClassInfo>();
                var types = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var info in ctx.Right)
                {
                    if (info is ErrorInfo e)
                    {
                        result.Add(new ErrorResult(e.Location, e.ErrorMessage));
                    }
                    else if (info is CreateClassInfo c)
                    {
                        classes.Add(c);
                        types.UnionWith(c.Types);
                    }
                    else if (info is InterceptInfo i)
                    {
                        intercepts.Add(i);
                        types.Add(i.TypeName);
                    }
                }

                using var sw = new StringWriter();
                var writer = new IndentedTextWriter(sw);

                writer.WriteLine("""
                    // <auto-generated />
                        
                    // Suppress some unused variables for now
                    #pragma warning disable CS0219

                    // Suppress warnings about [Obsolete] member usage in generated code.
                    #pragma warning disable CS0612, CS0618
                    """);
                writer.WriteLine();

                try
                {
                    var temp = new StringWriter();
                    var serializers = WriteSerializers(new IndentedTextWriter(temp), ctx.Left, types, token);

                    if (serializers is { })
                    {
                        if (classes.Any())
                        {
                            WriteClassContexts(writer, classes, serializers);
                        }

                        if (intercepts.Any())
                        {
                            sw.WriteLine();
                            sw.Write("""
                            #pragma warning disable CS9113

                            namespace System.Runtime.CompilerServices
                            {
                                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                                file sealed class InterceptsLocationAttribute(int version, string data) : Attribute
                                {
                                }
                            }
                            """);
                            sw.WriteLine();
                            sw.WriteLine();

                            WriteIntercepts(writer, intercepts, serializers);
                        }

                        sw.WriteLine();
                        sw.Write(temp);

                        var source = sw.ToString();

                        result.Add(new SourceResult("XmlGeneratedSerializers", source));
                    }
                }
                catch (Exception ex)
                {
                    result.Add(new ErrorResult(default, ex.Message));
                }

                return result;
            });

        // Register the source output
        context.RegisterSourceOutput(
            sourceGenerator,
                static (context, result) =>
                {
                    if (result is ErrorResult e)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SGEN0001", "XmlSerializerGenerator", $"Error running SGEN: {e.Message}", "XmlSerializerGenerator", DiagnosticSeverity.Error, true), e.Location));
                    }
                    else if (result is SourceResult s)
                    {
                        context.AddSource($"{s.Name}.g.cs", SourceText.From(s.Contents, Encoding.UTF8));
                    }
                });
    }

    private Dictionary<string, string>? WriteSerializers(IndentedTextWriter writer, Compilation compilation, IEnumerable<string> desiredTypes, CancellationToken token)
    {
        var metadataContext = new RoslynMetadataLoadContext(compilation);
        var r = new XmlReflectionImporter2(metadataContext);
        var types = new List<Type>();
        var mappings = new List<XmlMapping>();

        foreach (var t in desiredTypes)
        {
            var type = metadataContext.ResolveType(t);
            var mapping = r.ImportTypeMapping(type);
            mapping.SetKey(type.Name);
            mappings.Add(mapping);
            types.Add(type);
        }

        const string GeneratedAssemblyNamespace = "XmlSerializersGenerated";
        writer.WriteLine($"namespace {GeneratedAssemblyNamespace}");
        writer.WriteLine("{");
        writer.Indent++;

        var serializers = XmlSerializerImpl.GenerateSerializer(types, mappings, writer);

        if (serializers is { })
        {
            writer.WriteLine();

            writer.WriteLine("file static class Instances");
            writer.WriteLine("{");
            writer.Indent++;

            foreach (var serializer in serializers)
            {
                writer.Write("public static global::System.Xml.Serialization.XmlSerializer ");
                writer.Write(serializer.Key);
                writer.Write(" { get; } = new ");
                writer.Write(serializer.Value);
                writer.WriteLine("();");
            }

            writer.Indent--;
            writer.WriteLine("}");
        }

        writer.Indent--;
        writer.WriteLine("}");

        return serializers;
    }

    private void WriteIntercepts(IndentedTextWriter writer, IEnumerable<InterceptInfo> intercepts, Dictionary<string, string> serializers)
    {
        writer.WriteLine("namespace XmlSerializersGenerated");
        writer.WriteLine("{");
        writer.Indent++;

        writer.WriteLine("file static class InterceptedCalls");
        writer.WriteLine("{");
        writer.Indent++;

        foreach (var intercept in intercepts)
        {
            writer.Write("[global::System.Runtime.CompilerServices.InterceptsLocation(");
            writer.Write(intercept.InterceptedLocation.Version);
            writer.Write(", \"");
            writer.Write(intercept.InterceptedLocation.Data);
            writer.WriteLine("\")]");

            WriteGetMethod(writer, $"Get_{intercept.TypeName}", serializers);
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private void WriteGetMethod(IndentedTextWriter writer, string name, Dictionary<string, string> serializers)
    {
        writer.WriteLine($"public static global::System.Xml.Serialization.XmlSerializer {name}(Type type)");
        writer.WriteLine("{");
        writer.Indent++;

        foreach (var serializer in serializers)
        {
            writer.Write("if (type == typeof(");
            writer.Write(serializer.Key);
            writer.Write(")) return global::XmlSerializersGenerated.Instances.");
            writer.Write(serializer.Key);
            writer.WriteLine(";");
        }

        writer.WriteLine("return null;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private void WriteClassContexts(IndentedTextWriter writer, IEnumerable<CreateClassInfo> infos, Dictionary<string, string> serializers)
    {
        foreach (var info in infos)
        {
            if (!string.IsNullOrEmpty(info.Namespace))
            {
                writer.Write("namespace ");
                writer.WriteLine(info.Namespace);
                writer.WriteLine("{");
                writer.Indent++;
            }

            foreach (var c in info.ClassNames)
            {
                writer.Write("partial class ");
                writer.WriteLine(c);
                writer.WriteLine("{");
                writer.Indent++;
            }

            WriteGetMethod(writer, "Get", serializers);

            writer.WriteLineNoTabs(string.Empty);

            foreach (var serializer in serializers)
            {
                writer.Write("public static global::System.Xml.Serialization.XmlSerializer ");
                writer.Write(serializer.Key);
                writer.Write(" => global::XmlSerializersGenerated.Instances.");
                writer.Write(serializer.Key);
                writer.WriteLine(";");
            }

            foreach (var c in info.ClassNames)
            {
                writer.Indent--;
                writer.WriteLine("}");
            }


            if (!string.IsNullOrEmpty(info.Namespace))
            {
                writer.Indent--;
                writer.WriteLine("}");
            }
        }
    }

    private abstract record Result();

    private record ErrorResult(Location Location, string Message) : Result;

    private record SourceResult(string Name, string Contents) : Result;

    private abstract record Info(Location Location);

    interface ICreateInfo
    {
        string? Namespace { get; }

        //ImmutableArray<string> Types { get; }

        ImmutableArray<string> ClassNames { get; }
    }

    private record InterceptInfo(Location Location, InterceptableLocation InterceptedLocation, string MethodName, string TypeName) : Info(Location), ICreateInfo
    {
        public string? Namespace => "GeneratedSerializers";

        public ImmutableArray<string> ClassNames { get; } = [TypeName];
    }

    private record CreateClassInfo(
        Location Location,
        string? Namespace,
        ImmutableArray<string> ClassNames,
        ImmutableArray<string> Types) : Info(Location), ICreateInfo;

    private record ErrorInfo(Location Location, string ErrorMessage) : Info(Location);
}
