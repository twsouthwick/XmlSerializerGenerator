using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace XmlSerializer2.Test;

internal static class SourceVerifier
{
    public static void Run(string className, string poco, string expected)
    {
        Verify($@"
                using System;
                using System.Collections;
                using System.Xml.Serialization;

                [XmlSerializable(typeof({className}))]
                public partial class TestContext
                {{
                    public static void Run(System.IO.TextWriter writer)
                    {{
                        {className}.Serialize(writer, global::{className}.Create());
                    }}
                }}

                {poco}
                ",
            expected);
    }
    public static void Verify(string input, string expectedOutput)
    {
        var tree = CSharpSyntaxTree.ParseText(input);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Test",
            syntaxTrees: [tree],
            references: Basic.Reference.Assemblies.Net80.References.All,
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var generator = new XmlSerializerGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var ran = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

        var errors = ran.GetRunResult().Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        if (errors.Any())
        {
            Assert.Fail(string.Join("\n", errors.Select(e => e.GetMessage())));
        }

        using var ms = new MemoryStream();
        var result = updatedCompilation.Emit(
            peStream: ms);

        Assert.IsTrue(result.Success);

        ms.Position = 0;

        using var alc = new TestAssemblyLoadContext(ms);

        var output = alc.RunTest();

        Assert.AreEqual(expectedOutput, output);
    }

    private sealed class TestAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        private readonly Assembly _testAssembly;

        public TestAssemblyLoadContext(Stream testAssembly) : base("Test", isCollectible: true)
        {
            _testAssembly = LoadFromStream(testAssembly);
        }

        public void Dispose() => Unload();

        public string RunTest()
        {
            var writer = new StringWriter();
            var context = _testAssembly.GetType("TestContext");
            var method = context.GetRuntimeMethod("Run", [typeof(TextWriter)]);

            method.Invoke(null, [writer]);

            return writer.ToString();
        }
    }
}

