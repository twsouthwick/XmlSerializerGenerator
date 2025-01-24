using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace System.Xml.Serialization;

internal abstract class TypeModel
{
    private readonly TypeDesc _typeDesc;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private readonly Type _type;
    private readonly ModelScope _scope;

    protected TypeModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        TypeDesc typeDesc,
        ModelScope scope)
    {
        _scope = scope;
        _type = type;
        _typeDesc = typeDesc;
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    internal Type Type
    {
        get { return _type; }
    }

    internal ModelScope ModelScope
    {
        get { return _scope; }
    }

    internal TypeDesc TypeDesc
    {
        get { return _typeDesc; }
    }
}

internal sealed class ArrayModel : TypeModel
{
    internal ArrayModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

    internal TypeModel Element
    {
        [RequiresUnreferencedCode("Calls GetTypeModel")]
        get { return ModelScope.GetTypeModel(TypeScope.GetArrayElementType(Type, null)!); }
    }
}

internal sealed class PrimitiveModel : TypeModel
{
    internal PrimitiveModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }
}

internal sealed class SpecialModel : TypeModel
{
    internal SpecialModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }
}

internal sealed class StructModel : TypeModel
{
    internal StructModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

    internal MemberInfo[] GetMemberInfos()
    {
        // we use to return Type.GetMembers() here, the members were returned in a different order: fields first, properties last
        // Current System.Reflection code returns members in opposite order: properties first, then fields.
        // This code make sure that returns members in the Everett order.
        MemberInfo[] members = Type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        MemberInfo[] fieldsAndProps = new MemberInfo[members.Length];

        int cMember = 0;
        // first copy all non-property members over
        for (int i = 0; i < members.Length; i++)
        {
            if (!(members[i] is PropertyInfo))
            {
                fieldsAndProps[cMember++] = members[i];
            }
        }
        // now copy all property members over
        for (int i = 0; i < members.Length; i++)
        {
            if (members[i] is PropertyInfo)
            {
                fieldsAndProps[cMember++] = members[i];
            }
        }
        return fieldsAndProps;
    }

    [RequiresUnreferencedCode("calls GetFieldModel")]
    internal FieldModel? GetFieldModel(MemberInfo memberInfo)
    {
        FieldModel? model = null;
        if (memberInfo is FieldInfo)
            model = GetFieldModel((FieldInfo)memberInfo);
        else if (memberInfo is PropertyInfo)
            model = GetPropertyModel((PropertyInfo)memberInfo);
        if (model != null)
        {
            if (model.ReadOnly && model.FieldTypeDesc.Kind != TypeKind.Collection && model.FieldTypeDesc.Kind != TypeKind.Enumerable)
                return null;
        }
        return model;
    }

    private static void CheckSupportedMember(TypeDesc? typeDesc, MemberInfo member, Type type)
    {
        if (typeDesc == null)
            return;
        if (typeDesc.IsUnsupported)
        {
            typeDesc.Exception ??= new NotSupportedException(SR.Format(SR.XmlSerializerUnsupportedType, typeDesc.FullName));
            throw new InvalidOperationException(SR.Format(SR.XmlSerializerUnsupportedMember, $"{member.DeclaringType!.FullName}.{member.Name}", type.FullName), typeDesc.Exception);
        }
        CheckSupportedMember(typeDesc.BaseTypeDesc, member, type);
        CheckSupportedMember(typeDesc.ArrayElementTypeDesc, member, type);
    }

    [RequiresUnreferencedCode("calls GetTypeDesc")]
    private FieldModel? GetFieldModel(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsStatic) return null;
        if (fieldInfo.DeclaringType != Type) return null;

        TypeDesc typeDesc = ModelScope.TypeScope.GetTypeDesc(fieldInfo.FieldType, fieldInfo, true, false);
        if (fieldInfo.IsInitOnly && typeDesc.Kind != TypeKind.Collection && typeDesc.Kind != TypeKind.Enumerable)
            return null;

        CheckSupportedMember(typeDesc, fieldInfo, fieldInfo.FieldType);
        return new FieldModel(fieldInfo, fieldInfo.FieldType, typeDesc);
    }

    [RequiresUnreferencedCode("calls GetTypeDesc")]
    private FieldModel? GetPropertyModel(PropertyInfo propertyInfo)
    {
        if (propertyInfo.DeclaringType != Type) return null;
        if (CheckPropertyRead(propertyInfo))
        {
            TypeDesc typeDesc = ModelScope.TypeScope.GetTypeDesc(propertyInfo.PropertyType, propertyInfo, true, false);
            // Fix for CSDMain 100492, please contact arssrvlt if you need to change this line
            if (!propertyInfo.CanWrite && typeDesc.Kind != TypeKind.Collection && typeDesc.Kind != TypeKind.Enumerable)
                return null;
            CheckSupportedMember(typeDesc, propertyInfo, propertyInfo.PropertyType);
            return new FieldModel(propertyInfo, propertyInfo.PropertyType, typeDesc);
        }
        return null;
    }

    //CheckProperty
    internal static bool CheckPropertyRead(PropertyInfo propertyInfo)
    {
        if (!propertyInfo.CanRead) return false;

        MethodInfo getMethod = propertyInfo.GetMethod!;
        if (getMethod.IsStatic) return false;
        ParameterInfo[] parameters = getMethod.GetParameters();
        if (parameters.Length > 0) return false;
        return true;
    }
}

internal sealed class FieldModel
{
    private readonly SpecifiedAccessor _checkSpecified = SpecifiedAccessor.None;
    private readonly MemberInfo? _memberInfo;
    private readonly MemberInfo? _checkSpecifiedMemberInfo;
    private readonly MethodInfo? _checkShouldPersistMethodInfo;
    private readonly bool _checkShouldPersist;
    private readonly bool _readOnly;
    private readonly bool _isProperty;
    private readonly Type _fieldType;
    private readonly string _name;
    private readonly TypeDesc _fieldTypeDesc;

    internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist) :
        this(name, fieldType, fieldTypeDesc, checkSpecified, checkShouldPersist, false)
    {
    }
    internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist, bool readOnly)
    {
        _fieldTypeDesc = fieldTypeDesc;
        _name = name;
        _fieldType = fieldType;
        _checkSpecified = checkSpecified ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.None;
        _checkShouldPersist = checkShouldPersist;
        _readOnly = readOnly;
    }

    [RequiresUnreferencedCode("Calls GetField on MemberInfo type")]
    internal FieldModel(MemberInfo memberInfo, Type fieldType, TypeDesc fieldTypeDesc)
    {
        _name = memberInfo.Name;
        _fieldType = fieldType;
        _fieldTypeDesc = fieldTypeDesc;
        _memberInfo = memberInfo;
        _checkShouldPersistMethodInfo = memberInfo.DeclaringType!.GetMethod($"ShouldSerialize{memberInfo.Name}", Type.EmptyTypes);
        _checkShouldPersist = _checkShouldPersistMethodInfo != null;

        FieldInfo? specifiedField = memberInfo.DeclaringType.GetField($"{memberInfo.Name}Specified");
        if (specifiedField != null)
        {
            if (!specifiedField.FieldType.Equals(typeof(bool)))
            {
                throw new InvalidOperationException(SR.Format(SR.XmlInvalidSpecifiedType, specifiedField.Name, specifiedField.FieldType.FullName, typeof(bool).FullName));
            }
            _checkSpecified = specifiedField.IsInitOnly ? SpecifiedAccessor.ReadOnly : SpecifiedAccessor.ReadWrite;
            _checkSpecifiedMemberInfo = specifiedField;
        }
        else
        {
            PropertyInfo? specifiedProperty = memberInfo.DeclaringType.GetProperty($"{memberInfo.Name}Specified");
            if (specifiedProperty != null)
            {
                if (StructModel.CheckPropertyRead(specifiedProperty))
                {
                    _checkSpecified = specifiedProperty.CanWrite ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.ReadOnly;
                    _checkSpecifiedMemberInfo = specifiedProperty;
                }
                if (_checkSpecified != SpecifiedAccessor.None && specifiedProperty.PropertyType != typeof(bool))
                {
                    throw new InvalidOperationException(SR.Format(SR.XmlInvalidSpecifiedType, specifiedProperty.Name, specifiedProperty.PropertyType.FullName, typeof(bool).FullName));
                }
            }
        }
        if (memberInfo is PropertyInfo)
        {
            _readOnly = !((PropertyInfo)memberInfo).CanWrite;
            _isProperty = true;
        }
        else if (memberInfo is FieldInfo)
        {
            _readOnly = ((FieldInfo)memberInfo).IsInitOnly;
        }
    }

    internal string Name
    {
        get { return _name; }
    }

    internal Type FieldType
    {
        get { return _fieldType; }
    }

    internal TypeDesc FieldTypeDesc
    {
        get { return _fieldTypeDesc; }
    }

    internal bool CheckShouldPersist
    {
        get { return _checkShouldPersist; }
    }

    internal SpecifiedAccessor CheckSpecified
    {
        get { return _checkSpecified; }
    }

    internal MemberInfo? MemberInfo
    {
        get { return _memberInfo; }
    }
    internal MemberInfo? CheckSpecifiedMemberInfo
    {
        get { return _checkSpecifiedMemberInfo; }
    }
    internal MethodInfo? CheckShouldPersistMethodInfo
    {
        get { return _checkShouldPersistMethodInfo; }
    }

    internal bool ReadOnly
    {
        get { return _readOnly; }
    }

    internal bool IsProperty
    {
        get { return _isProperty; }
    }
}

internal sealed class ConstantModel
{
    private readonly FieldInfo _fieldInfo;
    private readonly long _value;

    internal ConstantModel(FieldInfo fieldInfo, long value)
    {
        _fieldInfo = fieldInfo;
        _value = value;
    }

    internal string Name
    {
        get { return _fieldInfo.Name; }
    }

    internal long Value
    {
        get { return _value; }
    }

    internal FieldInfo FieldInfo
    {
        get { return _fieldInfo; }
    }
}

internal sealed class EnumModel : TypeModel
{
    private ConstantModel[]? _constants;

    internal EnumModel(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope) { }

    internal ConstantModel[] Constants
    {
        get
        {
            if (_constants == null)
            {
                var list = new List<ConstantModel>();
                FieldInfo[] fields = Type.GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    ConstantModel? constant = GetConstantModel(fieldInfo);
                    if (constant != null) list.Add(constant);
                }
                _constants = list.ToArray();
            }
            return _constants;
        }
    }

    private static ConstantModel? GetConstantModel(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsSpecialName) return null;
        return new ConstantModel(fieldInfo, ((IConvertible)fieldInfo.GetValue(null)!).ToInt64(null));
    }
}

internal enum XmlAttributeFlags
{
    Enum = 0x1,
    Array = 0x2,
    Text = 0x4,
    ArrayItems = 0x8,
    Elements = 0x10,
    Attribute = 0x20,
    Root = 0x40,
    Type = 0x80,
    AnyElements = 0x100,
    AnyAttribute = 0x200,
    ChoiceIdentifier = 0x400,
    XmlnsDeclarations = 0x800,
}
