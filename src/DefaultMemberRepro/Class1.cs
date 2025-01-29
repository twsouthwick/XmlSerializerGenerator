using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DefaultMemberRepro
{
    [Generator]
    public class DefaultMemberGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static context =>
            {
                const string Source = """
            namespace Repro;

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            internal sealed class MyAttr : Attribute
            {
                public MyAttr(Type type)
                {
                    Type = type;
                }

                public Type Type { get; }
            }
            """;

                context.AddSource("MyAttr.g.cs", SourceText.From(Source, Encoding.UTF8));
            });

            var defaultMembers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "Repro.MyAttr",
                    (node, token) => true,
                    (context, token) =>
                    {
                        var location = context.TargetNode.GetLocation();

                        if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, token) is INamedTypeSymbol typeSymbol)
                        {
                            return typeSymbol.GetAttributes()
                                 .Where(a => a.AttributeClass!.Name.Equals("MyAttr"))
                                 .Select(a => a.ConstructorArguments[0].Value)
                                 .OfType<INamedTypeSymbol>()
                                 .Select(t =>
                                 {
                                     var name = t.ToDisplayString();
                                     var members = t.GetAttributes()
                                         .Where(a => a.AttributeClass!.Name == "DefaultMemberAttribute")
                                         .Select(a => ((INamedTypeSymbol)a.ConstructorArguments[0].Value).ToDisplayString())
                                         .ToImmutableArray();

                                     return new Result(name, members);
                                 })
                                 .ToImmutableArray();
                        }

                        return ImmutableArray<Result>.Empty;
                    });

            context.RegisterSourceOutput(
                defaultMembers,
                (context, members) =>
                {
                    foreach (var type in members)
                    {
                        if (type.DefaultMembers.Length == 0)
                        {

                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("D1", "DefaultMember", $"No default members found for {type.Name}", "Category", DiagnosticSeverity.Error, true), Location.None));
                        }
                        else
                        {
                            var defaultMembers = string.Join(", ", type.DefaultMembers);

                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("D1", "DefaultMember", $"Found '{type.Name}' default members: '{defaultMembers}'", "Category", DiagnosticSeverity.Warning, true), Location.None));
                        }
                    }
                });
        }

        private class Result(string name, ImmutableArray<string> defaultMembers)
        {
            public string Name => name;

            public ImmutableArray<string> DefaultMembers => defaultMembers;
        }
    }
}
