using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

using IndentedWriter = System.CodeDom.Compiler.IndentedTextWriter;
using XmlSerializer2;

namespace System.Xml.Serialization;

internal sealed class ReflectionAwareCodeGen
{
    private const string arrayMemberKey = "0";
    // reflectionVariables holds mapping between a reflection entity
    // referenced in the generated code (such as TypeInfo,
    // FieldInfo) and the variable which represent the entity (and
    // initialized before).
    // The types of reflection entity and corresponding key is
    // given below.
    // ----------------------------------------------------------------------------------
    // Entity           Key
    // ----------------------------------------------------------------------------------
    // Assembly         assembly.FullName
    // Type             CodeIdentifier.EscapedKeywords(type.FullName)
    // Field            fieldName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName>)
    // Property         propertyName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName)
    // ArrayAccessor    "0:"+CodeIdentifier.EscapedKeywords(typeof(Array).FullName)
    // MyCollectionAccessor     "0:"+CodeIdentifier.EscapedKeywords(typeof(MyCollection).FullName)
    // ----------------------------------------------------------------------------------
    private Hashtable _reflectionVariables = null!;
    private int _nextReflectionVariableNumber;
    private readonly IndentedWriter _writer;
    internal ReflectionAwareCodeGen(IndentedWriter writer)
    {
        _writer = writer;
    }

    [RequiresUnreferencedCode("calls GetTypeDesc")]
    internal void WriteReflectionInit(TypeScope scope)
    {
        foreach (Type type in scope.Types)
        {
            TypeDesc typeDesc = scope.GetTypeDesc(type);
            if (typeDesc.UseReflection)
                WriteTypeInfo(scope, typeDesc, type);
        }
    }

    [RequiresUnreferencedCode("TrimSerializationWarning")]
    private string WriteTypeInfo(TypeScope scope, TypeDesc typeDesc, Type type)
    {
        InitTheFirstTime();
        string typeFullName = typeDesc.CSharpName;
        string? typeVariable = (string?)_reflectionVariables[typeFullName];
        if (typeVariable != null)
            return typeVariable;

        if (type.IsArray)
        {
            typeVariable = GenerateVariableName("array", typeDesc.CSharpName);
            TypeDesc elementTypeDesc = typeDesc.ArrayElementTypeDesc!;
            if (elementTypeDesc.UseReflection)
            {
                string elementTypeVariable = WriteTypeInfo(scope, elementTypeDesc, scope.GetTypeFromTypeDesc(elementTypeDesc)!);
                _writer.WriteLine($"static {typeof(Type).FullName} {typeVariable} = {elementTypeVariable}.MakeArrayType();");
            }
            else
            {
                string assemblyVariable = WriteAssemblyInfo(type);
                _writer.Write($"static {typeof(Type).FullName} {typeVariable} = {assemblyVariable}.GetType(");
                WriteQuotedCSharpString(type.FullName);
                _writer.WriteLine(");");
            }
        }
        else
        {
            typeVariable = GenerateVariableName(nameof(type), typeDesc.CSharpName);

            Type? parameterType = Nullable.GetUnderlyingType(type);
            if (parameterType != null)
            {
                string parameterTypeVariable = WriteTypeInfo(scope, scope.GetTypeDesc(parameterType), parameterType);
                _writer.WriteLine($"static {typeof(Type).FullName} {typeVariable} = typeof(System.Nullable<>).MakeGenericType(new {typeof(Type).FullName}[] {{{parameterTypeVariable}}});");
            }
            else
            {
                string assemblyVariable = WriteAssemblyInfo(type);
                _writer.Write($"static {typeof(Type).FullName} {typeVariable} = {assemblyVariable}.GetType(");
                WriteQuotedCSharpString(type.FullName);
                _writer.WriteLine(");");
            }
        }

        _reflectionVariables.Add(typeFullName, typeVariable);

        TypeMapping? mapping = scope.GetTypeMappingFromTypeDesc(typeDesc);
        if (mapping != null)
            WriteMappingInfo(mapping, typeVariable, type);
        if (typeDesc.IsCollection || typeDesc.IsEnumerable)
        {// Arrays use the generic item_Array
            TypeDesc elementTypeDesc = typeDesc.ArrayElementTypeDesc!;
            if (elementTypeDesc.UseReflection)
                WriteTypeInfo(scope, elementTypeDesc, scope.GetTypeFromTypeDesc(elementTypeDesc)!);
            WriteCollectionInfo(typeVariable, typeDesc, type);
        }

        return typeVariable;
    }

    [MemberNotNull(nameof(_reflectionVariables))]
    private void InitTheFirstTime()
    {
        if (_reflectionVariables == null)
        {
            _reflectionVariables = new Hashtable();
            _writer.Write(string.Format(CultureInfo.InvariantCulture, HelperClassesForUseReflection,
                "object", "string", typeof(Type).FullName,
                typeof(FieldInfo).FullName, typeof(PropertyInfo).FullName));

            WriteDefaultIndexerInit(typeof(IList), typeof(Array).FullName!, false, false);
        }
    }

    private void WriteMappingInfo(TypeMapping mapping, string typeVariable,
        [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type)
    {
        string typeFullName = mapping.TypeDesc!.CSharpName;
        if (mapping is StructMapping)
        {
            StructMapping structMapping = (mapping as StructMapping)!;
            for (int i = 0; i < structMapping.Members!.Length; i++)
            {
                MemberMapping member = structMapping.Members[i];
                WriteMemberInfo(type, typeFullName, typeVariable, member.Name);
                if (member.CheckShouldPersist)
                {
                    string memberName = $"ShouldSerialize{member.Name}";
                    WriteMethodInfo(typeFullName, typeVariable, memberName, false);
                }
#if FALSE
                if (member.CheckSpecified != SpecifiedAccessor.None)
                {
                    string memberName = $"{member.Name}Specified";
                    WriteMemberInfo(type, typeFullName, typeVariable, memberName);
                }
#endif
                if (member.ChoiceIdentifier != null)
                {
                    string memberName = member.ChoiceIdentifier.MemberName!;
                    WriteMemberInfo(type, typeFullName, typeVariable, memberName);
                }
            }
        }
        else if (mapping is EnumMapping)
        {
            FieldInfo[] enumFields = type.GetFields();
            for (int i = 0; i < enumFields.Length; i++)
            {
                WriteMemberInfo(type, typeFullName, typeVariable, enumFields[i].Name);
            }
        }
    }
    private void WriteCollectionInfo(string typeVariable, TypeDesc typeDesc,
        [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type)
    {
        string typeFullName = CodeIdentifier2.GetCSharpName(type);
        string elementTypeFullName = typeDesc.ArrayElementTypeDesc!.CSharpName;
        bool elementUseReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
        if (typeDesc.IsCollection)
        {
            WriteDefaultIndexerInit(type, typeFullName, typeDesc.UseReflection, elementUseReflection);
        }
        else if (typeDesc.IsEnumerable)
        {
            if (typeDesc.IsGenericInterface)
            {
                WriteMethodInfo(typeFullName, typeVariable, "System.Collections.Generic.IEnumerable*", true);
            }
            else if (!typeDesc.IsPrivateImplementation)
            {
                WriteMethodInfo(typeFullName, typeVariable, "GetEnumerator", true);
            }
        }
        WriteMethodInfo(typeFullName, typeVariable, "Add", false, GetStringForTypeof(elementTypeFullName, elementUseReflection));
    }

    private string WriteAssemblyInfo(Type type)
    {
        string assemblyFullName = type.Assembly.FullName!;
        string? assemblyVariable = (string?)_reflectionVariables[assemblyFullName];
        if (assemblyVariable == null)
        {
            int iComma = assemblyFullName.IndexOf(',');
            string assemblyName = (iComma > -1) ? assemblyFullName.Substring(0, iComma) : assemblyFullName;
            assemblyVariable = GenerateVariableName("assembly", assemblyName);
            //writer.WriteLine("static "+ typeof(Assembly).FullName+" "+assemblyVariable+" = "+typeof(Assembly).FullName+".Load(");
            _writer.Write($"static {typeof(Assembly).FullName} {assemblyVariable} = ResolveDynamicAssembly(");
            WriteQuotedCSharpString(DynamicAssemblies.GetName(type.Assembly)/*assemblyFullName*/);
            _writer.WriteLine(");");
            _reflectionVariables.Add(assemblyFullName, assemblyVariable);
        }
        return assemblyVariable;
    }

    private string WriteMemberInfo(
        [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type, string escapedName, string typeVariable, string memberName)
    {
        MemberInfo[] memberInfos = type.GetMember(memberName);
        for (int i = 0; i < memberInfos.Length; i++)
        {
            if (memberInfos[i] is PropertyInfo)
            {
                string propVariable = GenerateVariableName("prop", memberName);
                _writer.Write($"static XSPropInfo {propVariable} = new XSPropInfo({typeVariable}, ");
                WriteQuotedCSharpString(memberName);
                _writer.WriteLine(");");
                _reflectionVariables.Add($"{memberName}:{escapedName}", propVariable);
                return propVariable;
            }
            else if (memberInfos[i] is FieldInfo)
            {
                string fieldVariable = GenerateVariableName("field", memberName);
                _writer.Write($"static XSFieldInfo {fieldVariable} = new XSFieldInfo({typeVariable}, ");
                WriteQuotedCSharpString(memberName);
                _writer.WriteLine(");");
                _reflectionVariables.Add($"{memberName}:{escapedName}", fieldVariable);
                return fieldVariable;
            }
        }
        throw new InvalidOperationException(SR.Format(SR.XmlSerializerUnsupportedType, memberInfos[0]));
    }

    private string WriteMethodInfo(string escapedName, string typeVariable, string memberName, bool isNonPublic, params string[] paramTypes)
    {
        string methodVariable = GenerateVariableName("method", memberName);
        _writer.Write($"static {typeof(MethodInfo).FullName} {methodVariable} = {typeVariable}.GetMethod(");
        WriteQuotedCSharpString(memberName);
        _writer.Write(", ");

        string bindingFlags = typeof(BindingFlags).FullName!;
        _writer.Write(bindingFlags);
        _writer.Write(".Public | ");
        _writer.Write(bindingFlags);
        _writer.Write(".Instance | ");
        _writer.Write(bindingFlags);
        _writer.Write(".Static");

        if (isNonPublic)
        {
            _writer.Write(" | ");
            _writer.Write(bindingFlags);
            _writer.Write(".NonPublic");
        }
        _writer.Write(", null, ");
        _writer.Write($"new {typeof(Type).FullName}[] {{ ");
        for (int i = 0; i < paramTypes.Length; i++)
        {
            _writer.Write(paramTypes[i]);
            if (i < (paramTypes.Length - 1))
                _writer.Write(", ");
        }
        _writer.WriteLine("}, null);");
        _reflectionVariables.Add($"{memberName}:{escapedName}", methodVariable);
        return methodVariable;
    }

    private string WriteDefaultIndexerInit(
        [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type, string escapedName, bool collectionUseReflection, bool elementUseReflection)
    {
        string itemVariable = GenerateVariableName("item", escapedName);
        PropertyInfo defaultIndexer = TypeScope.GetDefaultIndexer(type, null);
        _writer.Write("static XSArrayInfo ");
        _writer.Write(itemVariable);
        _writer.Write("= new XSArrayInfo(");
        _writer.Write(GetStringForTypeof(CodeIdentifier2.GetCSharpName(type), collectionUseReflection));
        _writer.Write(".GetProperty(");
        WriteQuotedCSharpString(defaultIndexer.Name);
        _writer.Write(",");
        //defaultIndexer.PropertyType is same as TypeDesc.ElementTypeDesc
        _writer.Write(GetStringForTypeof(CodeIdentifier2.GetCSharpName(defaultIndexer.PropertyType), elementUseReflection));
        _writer.Write(",new ");
        _writer.Write(typeof(Type[]).FullName);
        _writer.WriteLine("{typeof(int)}));");
        _reflectionVariables.Add($"{arrayMemberKey}:{escapedName}", itemVariable);
        return itemVariable;
    }

    private string GenerateVariableName(string prefix, string fullName)
    {
        ++_nextReflectionVariableNumber;
        return $"{prefix}{_nextReflectionVariableNumber}_{CodeIdentifier2.MakeValidInternal(fullName.Replace('.', '_'))}";
    }
    internal string? GetReflectionVariable(string typeFullName, string? memberName)
    {
        string key;
        if (memberName == null)
            key = typeFullName;
        else
            key = $"{memberName}:{typeFullName}";
        return (string?)_reflectionVariables[key];
    }


    internal string GetStringForMethodInvoke(string obj, string escapedTypeName, string methodName, bool useReflection, params string[] args)
    {
        StringBuilder sb = new StringBuilder();
        if (useReflection)
        {
            sb.Append(GetReflectionVariable(escapedTypeName, methodName));
            sb.Append(".Invoke(");
            sb.Append(obj);
            sb.Append(", new object[] {");
        }
        else
        {
            sb.Append(obj);
            sb.Append(".@");
            sb.Append(methodName);
            sb.Append('(');
        }
        for (int i = 0; i < args.Length; i++)
        {
            if (i != 0)
                sb.Append(", ");
            sb.Append(args[i]);
        }
        if (useReflection)
            sb.Append("})");
        else
            sb.Append(')');
        return sb.ToString();
    }

    internal string GetStringForEnumCompare(EnumMapping mapping, string memberName, bool useReflection)
    {
        if (!useReflection)
        {
            CodeIdentifier2.CheckValidIdentifier(memberName);
            return $"{mapping.TypeDesc!.CSharpName}.@{memberName}";
        }
        string memberAccess = GetStringForEnumMember(mapping.TypeDesc!.CSharpName, memberName, useReflection);
        return GetStringForEnumLongValue(memberAccess, useReflection);
    }
    internal static string GetStringForEnumLongValue(string variable, bool useReflection)
    {
        if (useReflection)
            return $"{typeof(Convert).FullName}.ToInt64({variable})";
        return $"(({typeof(long).FullName}){variable})";
    }

    internal string GetStringForTypeof(string typeFullName, bool useReflection)
    {
        if (useReflection)
        {
            return GetReflectionVariable(typeFullName, null)!;
        }
        else
        {
            return $"typeof({typeFullName})";
        }
    }
    internal string GetStringForMember(string obj, string memberName, TypeDesc typeDesc)
    {
        if (!typeDesc.UseReflection)
            return $"{obj}.@{memberName}";

        while (typeDesc != null)
        {
            string typeFullName = typeDesc.CSharpName;
            string? memberInfoName = GetReflectionVariable(typeFullName, memberName);
            if (memberInfoName != null)
                return $"{memberInfoName}[{obj}]";
            // member may be part of the basetype
            typeDesc = typeDesc.BaseTypeDesc!;
            if (typeDesc != null && !typeDesc.UseReflection)
                return $"(({typeDesc.CSharpName}){obj}).@{memberName}";
        }
        //throw GetReflectionVariableException(saveTypeDesc.CSharpName,memberName);
        // NOTE, sowmys:Must never happen. If it does let the code
        // gen continue to help debugging what's gone wrong.
        // Eventually the compilation will fail.
        return $"[{obj}]";
    }
    /*
    Exception GetReflectionVariableException(string typeFullName, string memberName){
        string key;
        if (memberName == null)
            key = typeFullName;
        else
            key = memberName+":"+typeFullName;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (object varAvail in reflectionVariables.Keys){
            sb.Append(varAvail.ToString());
            sb.Append("\n");
        }
        return new Exception("No reflection variable for " + key + "\nAvailable keys\n"+sb.ToString());
    }*/

    internal string GetStringForEnumMember(string typeFullName, string memberName, bool useReflection)
    {
        if (!useReflection)
            return $"{typeFullName}.@{memberName}";

        string? memberInfoName = GetReflectionVariable(typeFullName, memberName);
        return $"{memberInfoName}[null]";
    }

    internal string GetStringForArrayMember(string arrayName, string subscript, TypeDesc arrayTypeDesc)
    {
        if (!arrayTypeDesc.UseReflection)
        {
            return $"{arrayName}[{subscript}]";
        }
        string typeFullName = arrayTypeDesc.IsCollection ? arrayTypeDesc.CSharpName : typeof(Array).FullName!;
        string? arrayInfo = GetReflectionVariable(typeFullName, arrayMemberKey);
        return $"{arrayInfo}[{arrayName}, {subscript}]";
    }
    internal string GetStringForMethod(string obj, string typeFullName, string memberName, bool useReflection)
    {
        if (!useReflection)
            return $"{obj}.{memberName}(";

        string? memberInfoName = GetReflectionVariable(typeFullName, memberName);
        return $"{memberInfoName}.Invoke({obj}, new object[]{{";
    }
    internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast)
    {
        return GetStringForCreateInstance(escapedTypeName, useReflection, ctorInaccessible, cast, string.Empty);
    }

    internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast, string arg)
    {
        if (!useReflection && !ctorInaccessible)
            return $"new {escapedTypeName}({arg})";
        return GetStringForCreateInstance(GetStringForTypeof(escapedTypeName, useReflection), cast && !useReflection ? escapedTypeName : null, ctorInaccessible, arg);
    }

    internal static string GetStringForCreateInstance(string type, string? cast, bool nonPublic, string? arg)
    {
        StringBuilder createInstance = new StringBuilder();
        if (cast != null && cast.Length > 0)
        {
            createInstance.Append('(');
            createInstance.Append(cast);
            createInstance.Append(')');
        }
        createInstance.Append(typeof(Activator).FullName);
        createInstance.Append(".CreateInstance(");
        createInstance.Append(type);
        createInstance.Append(", ");
        string bindingFlags = typeof(BindingFlags).FullName!;
        createInstance.Append(bindingFlags);
        createInstance.Append(".Instance | ");
        createInstance.Append(bindingFlags);
        createInstance.Append(".Public | ");
        createInstance.Append(bindingFlags);
        createInstance.Append(".CreateInstance");

        if (nonPublic)
        {
            createInstance.Append(" | ");
            createInstance.Append(bindingFlags);
            createInstance.Append(".NonPublic");
        }

        if (string.IsNullOrEmpty(arg))
        {
            createInstance.Append(", null, new object[0], null)");
        }
        else
        {
            createInstance.Append(", null, new object[] { ");
            createInstance.Append(arg);
            createInstance.Append(" }, null)");
        }
        return createInstance.ToString();
    }

    internal void WriteLocalDecl(string typeFullName, string variableName, string initValue, bool useReflection)
    {
        if (useReflection)
            typeFullName = "object";
        _writer.Write(typeFullName);
        _writer.Write(" ");
        _writer.Write(variableName);
        if (initValue != null)
        {
            _writer.Write(" = ");
            if (!useReflection && initValue != "null")
            {
                _writer.Write($"({typeFullName})");
            }
            _writer.Write(initValue);
        }
        _writer.WriteLine(";");
    }

    internal void WriteCreateInstance(string escapedName, string source, bool useReflection, bool ctorInaccessible)
    {
        _writer.Write(useReflection ? "object" : escapedName);
        _writer.Write(" ");
        _writer.Write(source);
        _writer.Write(" = ");
        _writer.Write(GetStringForCreateInstance(escapedName, useReflection, ctorInaccessible, !useReflection && ctorInaccessible));
        _writer.WriteLine(";");
    }
    internal void WriteInstanceOf(string source, string escapedTypeName, bool useReflection)
    {
        if (!useReflection)
        {
            _writer.Write(source);
            _writer.Write(" is ");
            _writer.Write(escapedTypeName);
            return;
        }
        _writer.Write(GetReflectionVariable(escapedTypeName, null));
        _writer.Write(".IsAssignableFrom(");
        _writer.Write(source);
        _writer.Write(".GetType())");
    }

    internal void WriteArrayLocalDecl(string typeName, string variableName, string? initValue, TypeDesc arrayTypeDesc)
    {
        if (arrayTypeDesc.UseReflection)
        {
            if (arrayTypeDesc.IsEnumerable)
                typeName = typeof(IEnumerable).FullName!;
            else if (arrayTypeDesc.IsCollection)
                typeName = typeof(ICollection).FullName!;
            else
                typeName = typeof(Array).FullName!;
        }
        _writer.Write(typeName);
        _writer.Write(" ");
        _writer.Write(variableName);
        if (initValue != null)
        {
            _writer.Write(" = ");
            if (initValue != "null")
                _writer.Write($"({typeName})");
            _writer.Write(initValue);
        }
        _writer.WriteLine(";");
    }
    internal void WriteEnumCase(string fullTypeName, ConstantMapping c, bool useReflection)
    {
        _writer.Write("case ");
        if (useReflection)
        {
            _writer.Write(c.Value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            _writer.Write(fullTypeName);
            _writer.Write(".@");
            CodeIdentifier2.CheckValidIdentifier(c.Name);
            _writer.Write(c.Name);
        }
        _writer.Write(": ");
    }
    internal void WriteTypeCompare(string variable, string escapedTypeName, bool useReflection)
    {
        _writer.Write(variable);
        _writer.Write(" == ");
        _writer.Write(GetStringForTypeof(escapedTypeName, useReflection));
    }
    internal void WriteArrayTypeCompare(string variable, string escapedTypeName, string elementTypeName, bool useReflection)
    {
        if (!useReflection)
        {
            _writer.Write(variable);
            _writer.Write(" == typeof(");
            _writer.Write(escapedTypeName);
            _writer.Write(")");
            return;
        }
        _writer.Write(variable);
        _writer.Write(".IsArray ");
        _writer.Write(" && ");
        WriteTypeCompare($"{variable}.GetElementType()", elementTypeName, useReflection);
    }

    internal static void WriteQuotedCSharpString(IndentedWriter writer, string? value)
    {
        if (value == null)
        {
            writer.Write("null");
            return;
        }
        writer.Write("@\"");
        foreach (char ch in value)
        {
            if (ch < 32)
            {
                if (ch == '\r')
                    writer.Write("\\r");
                else if (ch == '\n')
                    writer.Write("\\n");
                else if (ch == '\t')
                    writer.Write("\\t");
                else
                {
                    byte b = (byte)ch;
                    writer.Write("\\x");
                    writer.Write(HexConverter.ToCharUpper(b >> 4));
                    writer.Write(HexConverter.ToCharUpper(b));
                }
            }
            else if (ch == '\"')
            {
                writer.Write("\"\"");
            }
            else
            {
                writer.Write(ch);
            }
        }
        writer.Write("\"");
    }

    internal void WriteQuotedCSharpString(string? value)
    {
        WriteQuotedCSharpString(_writer, value);
    }

    private const string HelperClassesForUseReflection = @"
    sealed class XSFieldInfo {{
       {3} fieldInfo;
        public XSFieldInfo({2} t, {1} memberName){{
            fieldInfo = t.GetField(memberName);
        }}
        public {0} this[{0} o] {{
            get {{
                return fieldInfo.GetValue(o);
            }}
            set {{
                fieldInfo.SetValue(o, value);
            }}
        }}
 
    }}
    sealed class XSPropInfo {{
        {4} propInfo;
        public XSPropInfo({2} t, {1} memberName){{
            propInfo = t.GetProperty(memberName);
        }}
        public {0} this[{0} o] {{
            get {{
                return propInfo.GetValue(o, null);
            }}
            set {{
                propInfo.SetValue(o, value, null);
            }}
        }}
    }}
    sealed class XSArrayInfo {{
        {4} propInfo;
        public XSArrayInfo({4} propInfo){{
            this.propInfo = propInfo;
        }}
        public {0} this[{0} a, int i] {{
            get {{
                return propInfo.GetValue(a, new {0}[]{{i}});
            }}
            set {{
                propInfo.SetValue(a, value, new {0}[]{{i}});
            }}
        }}
    }}
";
}
