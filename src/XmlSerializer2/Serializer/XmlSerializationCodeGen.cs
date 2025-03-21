﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XmlSerializer2;
using IndentedWriter = System.CodeDom.Compiler.IndentedTextWriter;

namespace System.Xml.Serialization;

internal class XmlSerializationCodeGen
{
    private readonly IndentedWriter _writer;
    private int _nextMethodNumber;
    private readonly Hashtable _methodNames = new Hashtable();
    private readonly ReflectionAwareCodeGen _raCodeGen;
    private readonly TypeScope[] _scopes;
    private readonly TypeDesc? _stringTypeDesc;
    private readonly TypeDesc? _qnameTypeDesc;
    private readonly string _access;
    private readonly string _className;
    private TypeMapping[]? _referencedMethods;
    private int _references;
    private readonly Hashtable _generatedMethods = new Hashtable();

    [RequiresUnreferencedCode("Calls GetTypeDesc")]
    internal XmlSerializationCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className)
    {
        _writer = writer;
        _scopes = scopes;
        if (scopes.Length > 0)
        {
            _stringTypeDesc = scopes[0].GetTypeDesc(typeof(string));
            _qnameTypeDesc = scopes[0].GetTypeDesc(typeof(XmlQualifiedName));
        }
        _raCodeGen = new ReflectionAwareCodeGen(writer);
        _className = className;
        _access = access;
    }

    internal IndentedWriter Writer { get { return _writer; } }
    internal int NextMethodNumber { get { return _nextMethodNumber; } set { _nextMethodNumber = value; } }
    internal ReflectionAwareCodeGen RaCodeGen { get { return _raCodeGen; } }
    internal TypeDesc? StringTypeDesc { get { return _stringTypeDesc; } }
    internal TypeDesc? QnameTypeDesc { get { return _qnameTypeDesc; } }
    internal string ClassName { get { return _className; } }
    internal string Access { get { return _access; } }
    internal TypeScope[] Scopes { get { return _scopes; } }
    internal Hashtable MethodNames { get { return _methodNames; } }
    internal Hashtable GeneratedMethods { get { return _generatedMethods; } }

    [RequiresUnreferencedCode("calls WriteStructMethod")]
    internal virtual void GenerateMethod(TypeMapping mapping) { }

    [RequiresUnreferencedCode("calls GenerateMethod")]
    internal void GenerateReferencedMethods()
    {
        while (_references > 0)
        {
            TypeMapping mapping = _referencedMethods![--_references];
            GenerateMethod(mapping);
        }
    }

    internal string? ReferenceMapping(TypeMapping mapping)
    {
        if (!mapping.IsSoap)
        {
            if (_generatedMethods[mapping] == null)
            {
                _referencedMethods = EnsureArrayIndex(_referencedMethods!, _references);
                _referencedMethods[_references++] = mapping;
            }
        }
        return (string?)_methodNames[mapping];
    }

    private static TypeMapping[] EnsureArrayIndex(TypeMapping[] a, int index)
    {
        if (a == null) return new TypeMapping[32];
        if (index < a.Length) return a;
        TypeMapping[] b = new TypeMapping[a.Length + 32];
        Array.Copy(a, b, index);
        return b;
    }

    internal void WriteQuotedCSharpString(string? value)
    {
        _raCodeGen.WriteQuotedCSharpString(value);
    }

    internal void GenerateHashtableGetBegin(string privateName, string publicName)
    {
        _writer.Write(typeof(Hashtable).FullName);
        _writer.Write(" ");
        _writer.Write(privateName);
        _writer.WriteLine(" = null;");
        _writer.Write("public override ");
        _writer.Write(typeof(Hashtable).FullName);

        _writer.Write(" ");
        _writer.Write(publicName);
        _writer.WriteLine(" {");
        _writer.Indent++;

        _writer.WriteLine("get {");
        _writer.Indent++;

        _writer.Write("if (");
        _writer.Write(privateName);
        _writer.WriteLine(" == null) {");
        _writer.Indent++;

        _writer.Write(typeof(Hashtable).FullName);
        _writer.Write(" _tmp = new ");
        _writer.Write(typeof(Hashtable).FullName);
        _writer.WriteLine("();");
    }

    internal void GenerateHashtableGetEnd(string privateName)
    {
        _writer.Write("if (");
        _writer.Write(privateName);
        _writer.Write(" == null) ");
        _writer.Write(privateName);
        _writer.WriteLine(" = _tmp;");
        _writer.Indent--;
        _writer.WriteLine("}");

        _writer.Write("return ");
        _writer.Write(privateName);
        _writer.WriteLine(";");
        _writer.Indent--;
        _writer.WriteLine("}");

        _writer.Indent--;
        _writer.WriteLine("}");
    }
    internal void GeneratePublicMethods(string privateName, string publicName, string?[] methods, XmlMapping[] xmlMappings)
    {
        GenerateHashtableGetBegin(privateName, publicName);
        if (methods != null && methods.Length != 0 && xmlMappings != null && xmlMappings.Length == methods.Length)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i] == null)
                    continue;
                _writer.Write("_tmp[");
                WriteQuotedCSharpString(xmlMappings[i].Key);
                _writer.Write("] = ");
                WriteQuotedCSharpString(methods[i]);
                _writer.WriteLine(";");
            }
        }
        GenerateHashtableGetEnd(privateName);
    }

    internal void GenerateSupportedTypes(Type?[] types)
    {
        _writer.Write("public override ");
        _writer.Write(typeof(bool).FullName);
        _writer.Write(" CanSerialize(");
        _writer.Write(typeof(Type).FullName);
        _writer.WriteLine(" type) {");
        _writer.Indent++;
        Hashtable uniqueTypes = new Hashtable();
        for (int i = 0; i < types.Length; i++)
        {
            Type? type = types[i];

            if (type == null)
                continue;
            if (!type.IsPublic && !type.IsNestedPublic)
                continue;
            if (uniqueTypes[type] != null)
                continue;
            if (DynamicAssemblies.IsTypeDynamic(type))
                continue;
            if (type.IsGenericType || type.ContainsGenericParameters && DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))
                continue;
            uniqueTypes[type] = type;
            _writer.Write("if (type == typeof(");
            _writer.Write(CodeIdentifier2.GetCSharpName(type));
            _writer.WriteLine(")) return true;");
        }
        _writer.WriteLine("return false;");
        _writer.Indent--;
        _writer.WriteLine("}");
    }

    internal string GenerateBaseSerializer(string baseSerializer, string readerClass, string writerClass, CodeIdentifiers classes)
    {
        baseSerializer = CodeIdentifier.MakeValid(baseSerializer);
        baseSerializer = classes.AddUnique(baseSerializer, baseSerializer);

        _writer.WriteLine();
        _writer.Write("file abstract class ");
        _writer.Write(CodeIdentifier2.GetCSharpName(baseSerializer));
        _writer.Write(" : ");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializer).FullName);
        _writer.WriteLine(" {");
        _writer.Indent++;

        _writer.Write("protected override ");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializationReader).FullName);
        _writer.WriteLine(" CreateReader() {");
        _writer.Indent++;
        _writer.Write("return new ");
        _writer.Write(readerClass);
        _writer.WriteLine("();");
        _writer.Indent--;
        _writer.WriteLine("}");

        _writer.Write("protected override ");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializationWriter).FullName);
        _writer.WriteLine(" CreateWriter() {");
        _writer.Indent++;
        _writer.Write("return new ");
        _writer.Write(writerClass);
        _writer.WriteLine("();");
        _writer.Indent--;
        _writer.WriteLine("}");

        _writer.Indent--;
        _writer.WriteLine("}");

        return baseSerializer;
    }

    internal string GenerateTypedSerializer(string? readMethod, string? writeMethod, XmlMapping mapping, CodeIdentifiers classes, string baseSerializer, string readerClass, string writerClass)
    {
        string serializerName = CodeIdentifier.MakeValid(Accessor.UnescapeName(mapping.Accessor.Mapping!.TypeDesc!.Name));
        serializerName = classes.AddUnique($"{serializerName}Serializer", mapping);

        _writer.WriteLine();
        _writer.Write("file sealed class ");
        _writer.Write(CodeIdentifier2.GetCSharpName(serializerName));
        _writer.Write(" : ");
        _writer.Write(baseSerializer);
        _writer.WriteLine(" {");
        _writer.Indent++;

        _writer.WriteLine();
        _writer.Write("public override ");
        _writer.Write(typeof(bool).FullName);
        _writer.Write(" CanDeserialize(");
        _writer.Write(typeof(XmlReader).FullName);
        _writer.WriteLine(" xmlReader) {");
        _writer.Indent++;

        if (mapping.Accessor.Any)
        {
            _writer.WriteLine("return true;");
        }
        else
        {
            _writer.Write("return xmlReader.IsStartElement(");
            WriteQuotedCSharpString(mapping.Accessor.Name);
            _writer.Write(", ");
            WriteQuotedCSharpString(mapping.Accessor.Namespace);
            _writer.WriteLine(");");
        }
        _writer.Indent--;
        _writer.WriteLine("}");

        if (writeMethod != null)
        {
            _writer.WriteLine();
            _writer.Write("protected override void Serialize(object objectToSerialize, ");
            _writer.Write(typeof(System.Xml.Serialization.XmlSerializationWriter).FullName);
            _writer.WriteLine(" writer) {");
            _writer.Indent++;
            _writer.Write("((");
            _writer.Write(writerClass);
            _writer.Write(")writer).");
            _writer.Write(writeMethod);
            _writer.Write("(");
            if (mapping is XmlMembersMapping)
            {
                _writer.Write("(object[])");
            }
            _writer.WriteLine("objectToSerialize);");
            _writer.Indent--;
            _writer.WriteLine("}");
        }
        if (readMethod != null)
        {
            _writer.WriteLine();
            _writer.Write("protected override object Deserialize(");
            _writer.Write(typeof(System.Xml.Serialization.XmlSerializationReader).FullName);
            _writer.WriteLine(" reader) {");
            _writer.Indent++;
            _writer.Write("return ((");
            _writer.Write(readerClass);
            _writer.Write(")reader).");
            _writer.Write(readMethod);
            _writer.WriteLine("();");
            _writer.Indent--;
            _writer.WriteLine("}");
        }
        _writer.Indent--;
        _writer.WriteLine("}");

        return serializerName;
    }

    private void GenerateTypedSerializers(Dictionary<string, string> serializers)
    {
        string privateName = "typedSerializers";
        GenerateHashtableGetBegin(privateName, "TypedSerializers");

        foreach (string key in serializers.Keys)
        {
            _writer.Write("_tmp.Add(");
            WriteQuotedCSharpString(key);
            _writer.Write(", new ");
            _writer.Write((string?)serializers[key]);
            _writer.WriteLine("());");
        }
        GenerateHashtableGetEnd("typedSerializers");
    }

    //GenerateGetSerializer(serializers, xmlMappings);
    private void GenerateGetSerializer(Dictionary<string, string> serializers, XmlMapping[] xmlMappings)
    {
        _writer.Write("public override ");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializer).FullName);
        _writer.Write(" GetSerializer(");
        _writer.Write(typeof(Type).FullName);
        _writer.WriteLine(" type) {");
        _writer.Indent++;

        for (int i = 0; i < xmlMappings.Length; i++)
        {
            if (xmlMappings[i] is XmlTypeMapping)
            {
                Type? type = xmlMappings[i].Accessor.Mapping!.TypeDesc!.Type;
                if (type == null)
                    continue;
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;
                if (DynamicAssemblies.IsTypeDynamic(type))
                    continue;
                if (type.IsGenericType || type.ContainsGenericParameters && DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))
                    continue;
                _writer.Write("if (type == typeof(");
                _writer.Write(CodeIdentifier2.GetCSharpName(type));
                _writer.Write(")) return new ");
                _writer.Write((string?)serializers[xmlMappings[i].Key!]);
                _writer.WriteLine("();");
            }
        }
        _writer.WriteLine("return null;");
        _writer.Indent--;
        _writer.WriteLine("}");
    }

    internal void GenerateSerializerContract(XmlMapping[] xmlMappings, Type?[] types, string readerType, string?[] readMethods, string writerType, string?[] writerMethods, Dictionary<string, string> serializers)
    {
        _writer.WriteLine();
        _writer.Write("file class XmlSerializerContract : global::");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializerImplementation).FullName);
        _writer.WriteLine(" {");
        _writer.Indent++;

        _writer.Write("public override global::");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializationReader).FullName);
        _writer.Write(" Reader { get { return new ");
        _writer.Write(readerType);
        _writer.WriteLine("(); } }");

        _writer.Write("public override global::");
        _writer.Write(typeof(System.Xml.Serialization.XmlSerializationWriter).FullName);
        _writer.Write(" Writer { get { return new ");
        _writer.Write(writerType);
        _writer.WriteLine("(); } }");

        GeneratePublicMethods(nameof(readMethods), "ReadMethods", readMethods, xmlMappings);
        GeneratePublicMethods("writeMethods", "WriteMethods", writerMethods, xmlMappings);
        GenerateTypedSerializers(serializers);
        GenerateSupportedTypes(types);
        GenerateGetSerializer(serializers, xmlMappings);

        _writer.Indent--;
        _writer.WriteLine("}");
    }

    internal static bool IsWildcard(SpecialMapping mapping)
    {
        if (mapping is SerializableMapping)
            return ((SerializableMapping)mapping).IsAny;
        return mapping.TypeDesc!.CanBeElementValue;
    }
}
