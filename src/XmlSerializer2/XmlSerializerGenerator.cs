using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace XmlSerializer2;

[Generator]
public class XmlSerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax provider to find object creation expressions for XmlSerializer
        var xmlSerializerCreations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, token) => GetSemanticTargetForGeneration(ctx, token))
            .Where(static m => m is not null)
            .Collect();
        var sourceGenerator = context.CompilationProvider
            .Combine(xmlSerializerCreations)
            .Select((ctx, token) =>
            {
                try
                {
                    return new CompilationCreationInfo(Create(ctx.Left, ctx.Right!, token), null);
                }
                catch (Exception e)
                {
                    return new CompilationCreationInfo(null, e);
                }
            });
        // Register the source output
        context.RegisterSourceOutput(
            sourceGenerator,
            static (context, result) =>
            {
                if (result.Exception is not null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SGEN0001", "XmlSerializerGenerator", $"Error running SGEN: {result.Exception.Message}", "XmlSerializerGenerator", DiagnosticSeverity.Error, true), Location.None));
                }
                else if (result.Source is { } source)
                {
                    context.AddSource("XmlSerializer.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
    }

    private string Create(Compilation compilation, ImmutableArray<XmlSerializerCreationInfo> infos, CancellationToken token)
    {
        var metadataContext = new MetadataLoadContext(compilation);
        var r = new XmlReflectionImporter2(metadataContext);
        var types = new List<Type>();
        var mappings = new List<XmlMapping>();

        foreach (var info in infos)
        {
            var type = metadataContext.ResolveType(info.FullyQualifiedName);
            mappings.Add(r.ImportTypeMapping(type));
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
        XmlSerializerImpl.GenerateSerializer(types, mappings, writer);

        var s = sw.ToString();
        return s;
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // We are looking for object creation expressions
        return node is ObjectCreationExpressionSyntax;
    }

    private static XmlSerializerCreationInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
    {
        // We are looking for object creation expressions for XmlSerializer
        if (context.SemanticModel.GetOperation(context.Node, token) is not IObjectCreationOperation objectCreation)
        {
            return null;
        }

        if (objectCreation.Type?.ToDisplayString() == "System.Xml.Serialization.XmlSerializer" && objectCreation.Arguments is [{ Value: ITypeOfOperation { TypeOperand: { } type } }])
        {
            var location = context.Node.GetLocation();
            var fullyQualifiedName = type.ToDisplayString();
            return new XmlSerializerCreationInfo(location, fullyQualifiedName);
        }

        return null;
    }

    private record CompilationCreationInfo(string? Source, Exception? Exception);

    private class XmlSerializerCreationInfo
    {
        public Location Location { get; }

        public string FullyQualifiedName { get; }

        public List<XmlSerializerTypeInfo> Types { get; } = [];

        public XmlSerializerCreationInfo(Location location, string fullyQualifiedName)
        {
            Location = location;
            FullyQualifiedName = fullyQualifiedName;
        }
    }

    private record class XmlSerializerTypeInfo
    {
        public required string Name { get; init; }

        public bool IsArray { get; init; }
    }
}
