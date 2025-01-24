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
                [assembly:System.Security.AllowPartiallyTrustedCallers()]
                [assembly:System.Security.SecurityTransparent()]
                [assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
                namespace XmlSerializersGenerated {
                    
                    public class XmlSerializationWriterSomeClass : System.Xml.Serialization.XmlSerializationWriter {
                        
                        public void Write4_SomeClass(object o) {
                            WriteStartDocument();
                            if (o == null) {
                                WriteNullTagLiteral(@"SomeClass", @"");
                                return;
                            }
                            TopLevelElement();
                            Write3_SomeClass(@"SomeClass", @"", ((global::SomeClass)o), true, false);
                        }
                        
                        void Write3_SomeClass(string n, string ns, global::SomeClass o, bool isNullable, bool needType) {
                            if ((object)o == null) {
                                if (isNullable) WriteNullTagLiteral(n, ns);
                                return;
                            }
                            if (!needType) {
                                System.Type t = o.GetType();
                                if (t == typeof(global::SomeClass)) {
                                }
                                else {
                                    throw CreateUnknownTypeException(o);
                                }
                            }
                            WriteStartElement(n, ns, o, false, null);
                            if (needType) WriteXsiType(@"SomeClass", @"");
                            WriteEndElement(o);
                        }
                        
                        protected override void InitCallbacks() {
                        }
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
                        ( typeof(XmlSerializerGenerator), "XmlSerializer.g.cs", SourceText.From(generated, Encoding.UTF8))
                    }
                },
            });
        }
    }
}

