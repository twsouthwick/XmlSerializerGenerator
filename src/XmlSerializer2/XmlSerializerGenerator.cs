using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            .Where(static m => m is not null);

        // Register the source output
        context.RegisterSourceOutput(xmlSerializerCreations, static (ctx, creation) => Execute(ctx, creation!));
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

    private static void Execute(SourceProductionContext context, XmlSerializerCreationInfo creationInfo)
    {
        const string GeneratedAssemblyNamespace = "XmlSerializersGenerated";

        using var sw = new StringWriter();
        var writer = new IndentedTextWriter(sw);

        writer.WriteLine("[assembly:System.Security.AllowPartiallyTrustedCallers()]");
        writer.WriteLine("[assembly:System.Security.SecurityTransparent()]");
        writer.WriteLine("[assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");


        var classes = new CodeIdentifiers();
        classes.AddUnique("XmlSerializationWriter", "XmlSerializationWriter");
        classes.AddUnique("XmlSerializationReader", "XmlSerializationReader");
        string? suffix = null;

        if (creationInfo.Types is [{ } first])
        {
            suffix = CodeIdentifier.MakeValid(first.Name);

            if (first.IsArray)
            {
                suffix += "Array";
            }
        }


        writer.WriteLine($"namespace {GeneratedAssemblyNamespace} {{");
        writer.Indent++;
        writer.WriteLine();

        string writerClass = $"XmlSerializationWriter{suffix}";
        writerClass = classes.AddUnique(writerClass, writerClass);
        //var writerCodeGen = new XmlSerializationWriterCodeGen(writer, scopes, "public", writerClass);
        //writerCodeGen.GenerateBegin();
        //string?[] writeMethodNames = new string[xmlMappings.Length];

        //for (int i = 0; i < xmlMappings.Length; i++)
        //{
        //    writeMethodNames[i] = writerCodeGen.GenerateElement(xmlMappings[i]);
        //}
        //writerCodeGen.GenerateEnd();
        //writer.WriteLine();


       
        //context.AddSource($"XmlSerializer.{className}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

        //string readerClass = $"XmlSerializationReader{suffix}";
        //readerClass = classes.AddUnique(readerClass, readerClass);
        //var readerCodeGen = new XmlSerializationReaderCodeGen(writer, scopes, "public", readerClass);
        //readerCodeGen.GenerateBegin();
        //string?[] readMethodNames = new string[xmlMappings.Length];
        //for (int i = 0; i < xmlMappings.Length; i++)
        //{
        //    readMethodNames[i] = readerCodeGen.GenerateElement(xmlMappings[i])!;
        //}

        //readerCodeGen.GenerateEnd();

        //string baseSerializer = readerCodeGen.GenerateBaseSerializer("XmlSerializer1", readerClass, writerClass, classes);
        //var serializers = new Hashtable();
        //for (int i = 0; i < xmlMappings.Length; i++)
        //{
        //    if (serializers[xmlMappings[i].Key!] == null)
        //    {
        //        serializers[xmlMappings[i].Key!] = readerCodeGen.GenerateTypedSerializer(readMethodNames[i], writeMethodNames[i], xmlMappings[i], classes, baseSerializer, readerClass, writerClass);
        //    }
        //}

        //readerCodeGen.GenerateSerializerContract(xmlMappings, types!, readerClass, readMethodNames, writerClass, writeMethodNames, serializers);
        //writer.Indent--;
        //writer.WriteLine("}");

        //string codecontent = compiler.Source.ToString()!;
        //byte[] info = new UTF8Encoding(true).GetBytes(codecontent);
        //stream.Write(info, 0, info.Length);
        //stream.Flush();
        //return true;
    }

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
