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
                    public int Value { get; set; }

                    public int[] Values { get; set; }
                }
                """;

            var generated = """
                [assembly:System.Security.AllowPartiallyTrustedCallers()]
                [assembly:System.Security.SecurityTransparent()]
                [assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
                namespace XmlSerializersGenerated {
                    
                    public class XmlSerializationWriterSomeClass : System.Xml.Serialization.XmlSerializationWriter {
                        
                        public void Write3_SomeClass(object o) {
                            WriteStartDocument();
                            if (o == null) {
                                WriteNullTagLiteral(@"SomeClass", @"");
                                return;
                            }
                            TopLevelElement();
                            Write2_SomeClass(@"SomeClass", @"", ((global::SomeClass)o), true, false);
                        }
                        
                        void Write2_SomeClass(string n, string ns, global::SomeClass o, bool isNullable, bool needType) {
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
                            WriteElementStringRaw(@"Value", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@Value)));
                            WriteEndElement(o);
                        }
                        
                        protected override void InitCallbacks() {
                        }
                    }
                    
                    public class XmlSerializationReaderSomeClass : System.Xml.Serialization.XmlSerializationReader {
                        
                        public object Read3_SomeClass() {
                            object o = null;
                            Reader.MoveToContent();
                            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                do {
                                    if (((object) Reader.LocalName == (object)id1_SomeClass && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                        o = Read2_SomeClass(true, true);
                                        break;
                                    }
                                    throw CreateUnknownNodeException();
                                } while (false);
                            }
                            else {
                                UnknownNode(null, @":SomeClass");
                            }
                            return (object)o;
                        }
                        
                        global::SomeClass Read2_SomeClass(bool isNullable, bool checkType) {
                            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
                            bool isNull = false;
                            if (isNullable) isNull = ReadNull();
                            if (checkType) {
                            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id1_SomeClass && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
                            }
                            else {
                                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
                            }
                            }
                            if (isNull) return null;
                            global::SomeClass o;
                            o = new global::SomeClass();
                            System.Span<bool> paramsRead = stackalloc bool[1];
                            while (Reader.MoveToNextAttribute()) {
                                if (!IsXmlnsAttribute(Reader.Name)) {
                                    UnknownNode((object)o);
                                }
                            }
                            Reader.MoveToElement();
                            if (Reader.IsEmptyElement) {
                                Reader.Skip();
                                return o;
                            }
                            Reader.ReadStartElement();
                            Reader.MoveToContent();
                            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                    do {
                                        if (!paramsRead[0] && ((object) Reader.LocalName == (object)id3_Value && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            {
                                                o.@Value = System.Xml.XmlConvert.ToInt32(Reader.ReadElementString());
                                            }
                                            paramsRead[0] = true;
                                            break;
                                        }
                                        UnknownNode((object)o, @":Value");
                                    } while (false);
                                }
                                else {
                                    UnknownNode((object)o, @":Value");
                                }
                                Reader.MoveToContent();
                            }
                            ReadEndElement();
                            return o;
                        }
                        
                        protected override void InitCallbacks() {
                        }
                        
                        string id1_SomeClass;
                        string id2_Item;
                        string id3_Value;
                        
                        protected override void InitIDs() {
                            id2_Item = Reader.NameTable.Add(@"");
                            id1_SomeClass = Reader.NameTable.Add(@"SomeClass");
                            id3_Value = Reader.NameTable.Add(@"Value");
                        }
                    }
                    
                    public abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer {
                        protected override System.Xml.Serialization.XmlSerializationReader CreateReader() {
                            return new XmlSerializationReaderSomeClass();
                        }
                        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter() {
                            return new XmlSerializationWriterSomeClass();
                        }
                    }
                    
                    public sealed class SomeClassSerializer : XmlSerializer1 {
                        
                        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader) {
                            return xmlReader.IsStartElement(@"SomeClass", @"");
                        }
                        
                        protected override void Serialize(object objectToSerialize, System.Xml.Serialization.XmlSerializationWriter writer) {
                            ((XmlSerializationWriterSomeClass)writer).Write3_SomeClass(objectToSerialize);
                        }
                        
                        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader) {
                            return ((XmlSerializationReaderSomeClass)reader).Read3_SomeClass();
                        }
                    }
                    
                    public class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation {
                        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReaderSomeClass(); } }
                        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriterSomeClass(); } }
                        System.Collections.Hashtable readMethods = null;
                        public override System.Collections.Hashtable ReadMethods {
                            get {
                                if (readMethods == null) {
                                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                                    _tmp[@"SomeClass::"] = @"Read3_SomeClass";
                                    if (readMethods == null) readMethods = _tmp;
                                }
                                return readMethods;
                            }
                        }
                        System.Collections.Hashtable writeMethods = null;
                        public override System.Collections.Hashtable WriteMethods {
                            get {
                                if (writeMethods == null) {
                                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                                    _tmp[@"SomeClass::"] = @"Write3_SomeClass";
                                    if (writeMethods == null) writeMethods = _tmp;
                                }
                                return writeMethods;
                            }
                        }
                        System.Collections.Hashtable typedSerializers = null;
                        public override System.Collections.Hashtable TypedSerializers {
                            get {
                                if (typedSerializers == null) {
                                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                                    _tmp.Add(@"SomeClass::", new SomeClassSerializer());
                                    if (typedSerializers == null) typedSerializers = _tmp;
                                }
                                return typedSerializers;
                            }
                        }
                        public override System.Boolean CanSerialize(System.Type type) {
                            if (type == typeof(global::SomeClass)) return true;
                            return false;
                        }
                        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type) {
                            if (type == typeof(global::SomeClass)) return new SomeClassSerializer();
                            return null;
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

