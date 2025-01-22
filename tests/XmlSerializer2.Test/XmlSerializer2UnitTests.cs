using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Verify = CSharpSourceGeneratorVerifier<XmlSerializer2.XmlSerializerGenerator>;

namespace XmlSerializer2.Test
{
    [TestClass]
    public class XmlSerializer2UnitTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = """
                using System;
                using System.Xml.Serialization;

                class Program
                {
                    static void Main()
                    {
                        var serializer = new XmlSerializer(typeof(SomeClass));
                    }
                }

                public class SomeClass
                {
                }
                """;

            var generated = """
                using System.Xml.Serialization;

                namespace XmlSerializers.Generated
                {
                    internal class SomeClassXmlSerializer : XmlSerializer
                    {
                    }
                }
                """;

            await Verify.RunAsync(new()
            {
                TestState =
                {
                    Sources = { test },
                    GeneratedSources =
                    {
                        ( typeof(XmlSerializerGenerator), "XmlSerializer.SomeClass.g.cs", SourceText.From(generated, Encoding.UTF8))
                    }
                },
            });
        }
    }
}

