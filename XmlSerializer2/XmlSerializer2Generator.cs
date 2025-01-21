using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace XmlSerializer2
{
    [Generator]
    public class SimpleIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterSyntaxReceiver((ctx, cancellationToken) =>
            {
                var receiver = new SyntaxReceiver();
                ctx.RegisterForSyntaxNotifications(() => receiver);
            });

            // Register the source output
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null),
                static (ctx, source) => Execute(ctx, source!));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            // We are looking for classes
            return node is ClassDeclarationSyntax;
        }

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // We are looking for classes with the attribute [GenerateMethod]
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeSymbol &&
                        attributeSymbol.ContainingType.ToDisplayString() == "XmlSerializer2.GenerateMethodAttribute")
                    {
                        return classDeclaration;
                    }
                }
            }
            return null;
        }

        private static void Execute(SourceProductionContext context, ClassDeclarationSyntax classDeclaration)
        {
            var className = classDeclaration.Identifier.Text;
            var source = $@"
using System;

namespace {classDeclaration.SyntaxTree.FilePath}
{{
    public partial class {className}
    {{
        public void GeneratedMethod()
        {{
            Console.WriteLine(""Hello from generated code!"");
        }}
    }}
}}";
            context.AddSource($"{className}_generated.cs", SourceText.From(source, Encoding.UTF8));
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Any class with the [GenerateMethod] attribute is a candidate for generation
                if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
                    classDeclaration.AttributeLists.Count > 0)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class GenerateMethodAttribute : Attribute
    {
        public GenerateMethodAttribute()
        {
        }
    }
}
