using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace XmlSerializer2;

[Generator]
public class XmlSerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            const string Source = """
            namespace System.Xml.Serialization;

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            internal sealed class XmlSerializableAttribute : Attribute
            {
                public XmlSerializableAttribute(Type type)
                {
                    Type = type;
                }

                public Type Type { get; }
            }
            """;

            context.AddSource("XmlSerializableAttribute.g.cs", SourceText.From(Source, Encoding.UTF8));
        });

        // Register a syntax provider to find object creation expressions for XmlSerializer
        var xmlSerializerContext = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "System.Xml.Serialization.XmlSerializableAttribute",
                (node, token) => true,
                (context, token) =>
                {
                    var location = context.TargetNode.GetLocation();

                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, token) is INamedTypeSymbol typeSymbol)
                    {
                        var types = typeSymbol.GetAttributes()
                             .Where(a => a.AttributeClass!.Name.Equals("XmlSerializableAttribute"))
                             .Select(a => a.ConstructorArguments[0].Value)
                             .OfType<INamedTypeSymbol>()
                             .Select(t => t.ToDisplayString())
                             .ToImmutableArray();

                        var ns = typeSymbol.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() : null;

                        return (Info)new CreateInfo(location, ns, GetContainingClasses(typeSymbol), types);
                    }

                    return (Info)new ErrorInfo(location, "XML serialization generator must have a partial method with no parameters");

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
                })
            .Collect();

        var sourceGenerator = context.CompilationProvider
            .Combine(xmlSerializerContext)
            .SelectMany((ctx, token) =>
            {
                var result = new List<Result>();

                foreach (var info in ctx.Right)
                {
                    if (info is ErrorInfo e)
                    {
                        result.Add(new ErrorResult(e.Location, e.ErrorMessage));
                    }
                    else if (info is CreateInfo c)
                    {
                        try
                        {
                            var source = Create(ctx.Left, c, token);

                            result.Add(new SourceResult(GetFileName(c), source));
                        }
                        catch (Exception ex)
                        {
                            result.Add(new ErrorResult(c.Location, ex.Message));
                        }
                    }
                }

                return result;

                static string GetFileName(CreateInfo c)
                {
                    var sb = new StringBuilder();

                    if (!string.IsNullOrEmpty(c.Namespace))
                    {
                        sb.Append(c.Namespace);
                        sb.Append('_');
                    }

                    foreach (var cl in c.ClassNames)
                    {
                        sb.Append(cl);
                        sb.Append('_');
                    }

                    sb.Append("XmlSerializers");

                    return sb.ToString();
                }
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

    private string Create(Compilation compilation, CreateInfo info, CancellationToken token)
    {
        var metadataContext = new RoslynMetadataLoadContext(compilation);
        var r = new XmlReflectionImporter2(metadataContext);
        var types = new List<Type>();
        var mappings = new List<XmlMapping>();

        foreach (var t in info.Types)
        {
            var type = metadataContext.ResolveType(t);
            var mapping = r.ImportTypeMapping(type);
            mapping.SetKey(type.Name);
            mappings.Add(mapping);
            types.Add(type);
        }

        using var sw = new StringWriter();

        sw.WriteLine("""
            // <auto-generated />
                        
            // Suppress some unused variables for now
            #pragma warning disable CS0219

            // Suppress warnings about [Obsolete] member usage in generated code.
            #pragma warning disable CS0612, CS0618
            """);
        sw.WriteLine();

        var writer = new IndentedTextWriter(sw);

        if (!string.IsNullOrEmpty(info.Namespace))
        {
            writer.WriteLine($"namespace {info.Namespace};");
            writer.WriteLineNoTabs(string.Empty);
        }

        var serializers = XmlSerializerImpl.GenerateSerializer(types, mappings, writer);

        if (serializers is { })
        {
            WritePartialImplementation(writer, info, serializers);
        }

        var s = sw.ToString();
        return s;
    }

    private void WritePartialImplementation(IndentedTextWriter writer, CreateInfo info, Dictionary<string, string> serializers)
    {
        foreach (var c in info.ClassNames)
        {
            writer.Write("partial class ");
            writer.WriteLine(c);
            writer.WriteLine("{");
            writer.Indent++;
        }

        foreach (var serializer in serializers)
        {
            writer.Write("public static global::System.Xml.Serialization.XmlSerializer ");
            writer.Write(serializer.Key);
            writer.Write(" { get; } = new ");
            writer.Write(serializer.Value);
            writer.WriteLine("();");
        }

        foreach (var c in info.ClassNames)
        {
            writer.Indent--;
            writer.Write("}");
        }
    }

    private abstract record Result();

    private record ErrorResult(Location Location, string Message) : Result;

    private record SourceResult(string Name, string Contents) : Result;

    private abstract record Info(Location Location);

    private record CreateInfo(
        Location Location,
        string? Namespace,
        ImmutableArray<string> ClassNames,
        ImmutableArray<string> Types) : Info(Location);

    private record ErrorInfo(Location Location, string ErrorMessage) : Info(Location);
}
