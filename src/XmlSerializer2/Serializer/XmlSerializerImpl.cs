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

    internal static Dictionary<string, string>? GenerateSerializer(List<Type> types, List<XmlMapping> mappings, IndentedWriter writer)
    {
        if (types.Count == 0)
        {
            return null;
        }

        if (XmlMapping.IsShallow(mappings))
        {
            throw new InvalidOperationException(SR.XmlMelformMapping);
        }

        Assembly? assembly = null;
        foreach (Type type in types)
        {
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

    internal static Dictionary<string, string> GenerateSerializerToStream(List<XmlMapping> xmlMappings, List<Type> types, string? defaultNamespace, Assembly? assembly, Hashtable assemblies, IndentedWriter writer)
    {
        var compiler = new Compiler();
        var scopeTable = new Hashtable();

        foreach (XmlMapping mapping in xmlMappings)
            scopeTable[mapping.Scope!] = mapping;

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

        foreach (var t in types)
        {
            Compiler.AddImport(t, importedTypes);
        }

#if FALSE
        writer.WriteLine("[assembly:System.Security.AllowPartiallyTrustedCallers()]");
        writer.WriteLine("[assembly:System.Security.SecurityTransparent()]");
        writer.WriteLine("[assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");
#endif

        if (assembly != null && types.Count > 0)
        {
            for (int i = 0; i < types.Count; i++)
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

        if (types != null && types.Count == 1 && types[0] != null)
        {
            suffix = CodeIdentifier.MakeValid(types[0]!.Name);
            if (types[0]!.IsArray)
            {
                suffix += "Array";
            }
        }

        string writerClass = $"XmlSerializationWriter{suffix}";
        writerClass = classes.AddUnique(writerClass, writerClass);
        var writerCodeGen = new XmlSerializationWriterCodeGen(writer, scopes, "file", writerClass);
        writerCodeGen.GenerateBegin();
        string?[] writeMethodNames = new string[xmlMappings.Count];

        for (int i = 0; i < xmlMappings.Count; i++)
        {
            writeMethodNames[i] = writerCodeGen.GenerateElement(xmlMappings[i]);
        }

        writerCodeGen.GenerateEnd();
        writer.WriteLine();

        string readerClass = $"XmlSerializationReader{suffix}";
        readerClass = classes.AddUnique(readerClass, readerClass);
        var readerCodeGen = new XmlSerializationReaderCodeGen(writer, scopes, "file", readerClass);
        readerCodeGen.GenerateBegin();
        string?[] readMethodNames = new string[xmlMappings.Count];
        for (int i = 0; i < xmlMappings.Count; i++)
        {
            readMethodNames[i] = readerCodeGen.GenerateElement(xmlMappings[i])!;
        }

        readerCodeGen.GenerateEnd();

        string baseSerializer = readerCodeGen.GenerateBaseSerializer("XmlSerializer1", readerClass, writerClass, classes);
        var serializers = new Dictionary<string, string>();
        for (int i = 0; i < xmlMappings.Count; i++)
        {
            if (!serializers.ContainsKey(xmlMappings[i].Key!))
            {
                serializers[xmlMappings[i].Key!] = readerCodeGen.GenerateTypedSerializer(readMethodNames[i], writeMethodNames[i], xmlMappings[i], classes, baseSerializer, readerClass, writerClass);
            }
        }

        readerCodeGen.GenerateSerializerContract([.. xmlMappings], [.. types], readerClass, readMethodNames, writerClass, writeMethodNames, serializers);

        return serializers;
    }
}
