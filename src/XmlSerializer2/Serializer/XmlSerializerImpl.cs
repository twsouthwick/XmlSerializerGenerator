using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace XmlSerializer2;

using IndentedWriter = System.CodeDom.Compiler.IndentedTextWriter;

class XmlSerializerImpl
{
    private const string GeneratedAssemblyNamespace = "XmlSerializersGenerated";

    internal static bool GenerateSerializer(Type[]? types, XmlMapping[] mappings, IndentedWriter writer)
    {
        if (types == null || types.Length == 0)
            return false;

        //if (XmlMapping.IsShallow(mappings))
        //{
        //    throw new InvalidOperationException(SR.XmlMelformMapping);
        //}

        Assembly? assembly = null;
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            if (DynamicAssemblies.IsTypeDynamic(type))
            {
                throw new InvalidOperationException(SR.Format(SR.XmlPregenTypeDynamic, type.FullName));
            }

            if (assembly == null)
            {
                assembly = type.Assembly;
            }
            else if (type.Assembly != assembly)
            {
                string? nameOrLocation = assembly.Location;
                if (nameOrLocation == string.Empty)
                    nameOrLocation = assembly.FullName;
                throw new ArgumentException(SR.Format(SR.XmlPregenOrphanType, type.FullName, nameOrLocation), nameof(types));
            }
        }

        return GenerateSerializerToStream(mappings, types, null, assembly, new Hashtable(), writer);
    }

    internal static bool GenerateSerializerToStream(XmlMapping[] xmlMappings, Type?[] types, string? defaultNamespace, Assembly? assembly, Hashtable assemblies, IndentedWriter writer)
    {
        var compiler = new Compiler();
        var scopeTable = new Hashtable();
        //foreach (XmlMapping mapping in xmlMappings)
            //scopeTable[mapping.Scope!] = mapping;

        var scopes = new TypeScope[scopeTable.Keys.Count];
        scopeTable.Keys.CopyTo(scopes, 0);
        assemblies.Clear();
        var importedTypes = new Hashtable();

        foreach (TypeScope scope in scopes)
        {
            foreach (Type t in scope.Types)
            {
                Compiler.AddImport(t, importedTypes);
                Assembly a = t.Assembly;
                string name = a.FullName!;
                if (assemblies[name] != null)
                {
                    continue;
                }

                assemblies[name] = a;
            }
        }

        for (int i = 0; i < types.Length; i++)
        {
            Compiler.AddImport(types[i], importedTypes);
        }

        writer.WriteLine("[assembly:System.Security.AllowPartiallyTrustedCallers()]");
        writer.WriteLine("[assembly:System.Security.SecurityTransparent()]");
        writer.WriteLine("[assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");

        if (assembly != null && types.Length > 0)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type? type = types[i];
                if (type == null)
                {
                    continue;
                }

                if (DynamicAssemblies.IsTypeDynamic(type))
                {
                    throw new InvalidOperationException(SR.Format(SR.XmlPregenTypeDynamic, types[i]!.FullName));
                }
            }

            // TODO: Do we need this?
#if FALSE
            writer.Write("[assembly:");
            writer.Write(typeof(XmlSerializerVersionAttribute).FullName);
            writer.Write("(");
            writer.Write("ParentAssemblyId=");
            ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, GenerateAssemblyId(types[0]!));
            writer.Write(", Version=");
            ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, ThisAssembly.Version);
            if (defaultNamespace != null)
            {
                writer.Write(", Namespace=");
                ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, defaultNamespace);
            }

            writer.WriteLine(")]");
#endif
        }

        var classes = new CodeIdentifiers();
        classes.AddUnique("XmlSerializationWriter", "XmlSerializationWriter");
        classes.AddUnique("XmlSerializationReader", "XmlSerializationReader");
        string? suffix = null;

        if (types != null && types.Length == 1 && types[0] != null)
        {
            suffix = CodeIdentifier.MakeValid(types[0]!.Name);
            if (types[0]!.IsArray)
            {
                suffix += "Array";
            }
        }

        writer.WriteLine($"namespace {GeneratedAssemblyNamespace} {{");
        writer.Indent++;
        writer.WriteLine();

        string writerClass = $"XmlSerializationWriter{suffix}";
        writerClass = classes.AddUnique(writerClass, writerClass);
        var writerCodeGen = new XmlSerializationWriterCodeGen(writer, scopes, "public", writerClass);
        writerCodeGen.GenerateBegin();
        string?[] writeMethodNames = new string[xmlMappings.Length];

        for (int i = 0; i < xmlMappings.Length; i++)
        {
            writeMethodNames[i] = writerCodeGen.GenerateElement(xmlMappings[i]);
        }

        writerCodeGen.GenerateEnd();
        writer.WriteLine();

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
        //    if (serializers[xmlMappings[i].GetKey()!] == null)
        //    {
        //        serializers[xmlMappings[i].GetKey()!] = readerCodeGen.GenerateTypedSerializer(readMethodNames[i], writeMethodNames[i], xmlMappings[i], classes, baseSerializer, readerClass, writerClass);
        //    }
        //}

        //readerCodeGen.GenerateSerializerContract(xmlMappings, types!, readerClass, readMethodNames, writerClass, writeMethodNames, serializers);
        writer.Indent--;
        writer.WriteLine("}");

        return true;
    }
}
