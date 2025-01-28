using Roslyn.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable enable

namespace XmlSerializer2;

internal class AttributeProviders
{
    private static readonly Dictionary<string, Func<MetadataLoadContext, CustomAttributeData, object?>> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "System.ObsoleteAttribute", CreateSystem_ObsoleteAttribute },
        { "System.Reflection.DefaultMemberAttribute", CreateSystem_Reflection_DefaultMemberAttribute },
        { "System.Runtime.Serialization.DataContractAttribute", CreateSystem_Runtime_Serialization_DataContractAttribute },
        { "System.Runtime.Serialization.DataMemberAttribute", CreateSystem_Runtime_Serialization_DataMemberAttribute },
        { "System.Runtime.Serialization.KnownTypeAttribute", CreateSystem_Runtime_Serialization_KnownTypeAttribute },
        { "System.Xml.Serialization.XmlAnyElementAttribute", CreateSystem_Xml_Serialization_XmlAnyElementAttribute },
        { "System.Xml.Serialization.XmlArrayAttribute", CreateSystem_Xml_Serialization_XmlArrayAttribute },
        { "System.Xml.Serialization.XmlArrayItemAttribute", CreateSystem_Xml_Serialization_XmlArrayItemAttribute },
        { "System.Xml.Serialization.XmlAttributeAttribute", CreateSystem_Xml_Serialization_XmlAttributeAttribute },
        { "System.Xml.Serialization.XmlChoiceIdentifierAttribute", CreateSystem_Xml_Serialization_XmlChoiceIdentifierAttribute },
        { "System.Xml.Serialization.XmlElementAttribute", CreateSystem_Xml_Serialization_XmlElementAttribute },
        { "System.Xml.Serialization.XmlIgnoreAttribute", CreateSystem_Xml_Serialization_XmlIgnoreAttribute },
        { "System.Xml.Serialization.XmlIncludeAttribute", CreateSystem_Xml_Serialization_XmlIncludeAttribute },
        { "System.Xml.Serialization.XmlRootAttribute", CreateSystem_Xml_Serialization_XmlRootAttribute },
        { "System.Xml.Serialization.XmlSchemaProviderAttribute", CreateSystem_Xml_Serialization_XmlSchemaProviderAttribute },
        { "System.Xml.Serialization.XmlTextAttribute", CreateSystem_Xml_Serialization_XmlTextAttribute },
        { "System.Xml.Serialization.XmlTypeAttribute", CreateSystem_Xml_Serialization_XmlTypeAttribute },
    };

    public static object? Create(MetadataLoadContext context, CustomAttributeData data)
{
    if (_map.TryGetValue(data.AttributeType.FullName, out var func))
    {
        return func(context, data);
    }

    return null;
}

    private static global::System.ObsoleteAttribute? CreateSystem_ObsoleteAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.ObsoleteAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0, { ArgumentType: {} argType1 } arg1] when argType0.Equals(typeof(global::System.String)) && argType1.Equals(typeof(global::System.Boolean)) => new((global::System.String)arg0.Value, (global::System.Boolean)arg1.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        return attr;
    }

    private static global::System.Reflection.DefaultMemberAttribute? CreateSystem_Reflection_DefaultMemberAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Reflection.DefaultMemberAttribute? attr = data.ConstructorArguments switch
        {
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        return attr;
    }

    private static global::System.Runtime.Serialization.DataContractAttribute? CreateSystem_Runtime_Serialization_DataContractAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Runtime.Serialization.DataContractAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Runtime.Serialization.DataContractAttribute.IsReference) && named.TypedValue is { Value: global::System.Boolean named_IsReference })
            {
                attr.IsReference = named_IsReference;
            }
            else if (named.MemberName == nameof(global::System.Runtime.Serialization.DataContractAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Runtime.Serialization.DataContractAttribute.Name) && named.TypedValue is { Value: global::System.String named_Name })
            {
                attr.Name = named_Name;
            }
        }

        return attr;
    }

    private static global::System.Runtime.Serialization.DataMemberAttribute? CreateSystem_Runtime_Serialization_DataMemberAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Runtime.Serialization.DataMemberAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Runtime.Serialization.DataMemberAttribute.Name) && named.TypedValue is { Value: global::System.String named_Name })
            {
                attr.Name = named_Name;
            }
            else if (named.MemberName == nameof(global::System.Runtime.Serialization.DataMemberAttribute.Order) && named.TypedValue is { Value: global::System.Int32 named_Order })
            {
                attr.Order = named_Order;
            }
            else if (named.MemberName == nameof(global::System.Runtime.Serialization.DataMemberAttribute.IsRequired) && named.TypedValue is { Value: global::System.Boolean named_IsRequired })
            {
                attr.IsRequired = named_IsRequired;
            }
            else if (named.MemberName == nameof(global::System.Runtime.Serialization.DataMemberAttribute.EmitDefaultValue) && named.TypedValue is { Value: global::System.Boolean named_EmitDefaultValue })
            {
                attr.EmitDefaultValue = named_EmitDefaultValue;
            }
        }

        return attr;
    }

    private static global::System.Runtime.Serialization.KnownTypeAttribute? CreateSystem_Runtime_Serialization_KnownTypeAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Runtime.Serialization.KnownTypeAttribute? attr = data.ConstructorArguments switch
        {
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlAnyElementAttribute? CreateSystem_Xml_Serialization_XmlAnyElementAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlAnyElementAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0, { ArgumentType: {} argType1 } arg1] when argType0.Equals(typeof(global::System.String)) && argType1.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value, (global::System.String)arg1.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAnyElementAttribute.Name) && named.TypedValue is { Value: global::System.String named_Name })
            {
                attr.Name = named_Name;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAnyElementAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAnyElementAttribute.Order) && named.TypedValue is { Value: global::System.Int32 named_Order })
            {
                attr.Order = named_Order;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlArrayAttribute? CreateSystem_Xml_Serialization_XmlArrayAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlArrayAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayAttribute.ElementName) && named.TypedValue is { Value: global::System.String named_ElementName })
            {
                attr.ElementName = named_ElementName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayAttribute.IsNullable) && named.TypedValue is { Value: global::System.Boolean named_IsNullable })
            {
                attr.IsNullable = named_IsNullable;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayAttribute.Form) && named.TypedValue is { Value: global::System.Xml.Schema.XmlSchemaForm named_Form })
            {
                attr.Form = named_Form;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayAttribute.Order) && named.TypedValue is { Value: global::System.Int32 named_Order })
            {
                attr.Order = named_Order;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlArrayItemAttribute? CreateSystem_Xml_Serialization_XmlArrayItemAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlArrayItemAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0, { ArgumentType: {} argType1 } arg1] when argType0.Equals(typeof(global::System.String)) && argType1.Equals(typeof(global::System.Type)) => new((global::System.String)arg0.Value, (global::System.Type)arg1.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.Type) && named.TypedValue is { Value: global::System.Type named_Type })
            {
                attr.Type = named_Type;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.ElementName) && named.TypedValue is { Value: global::System.String named_ElementName })
            {
                attr.ElementName = named_ElementName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.NestingLevel) && named.TypedValue is { Value: global::System.Int32 named_NestingLevel })
            {
                attr.NestingLevel = named_NestingLevel;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.DataType) && named.TypedValue is { Value: global::System.String named_DataType })
            {
                attr.DataType = named_DataType;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.IsNullable) && named.TypedValue is { Value: global::System.Boolean named_IsNullable })
            {
                attr.IsNullable = named_IsNullable;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlArrayItemAttribute.Form) && named.TypedValue is { Value: global::System.Xml.Schema.XmlSchemaForm named_Form })
            {
                attr.Form = named_Form;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlAttributeAttribute? CreateSystem_Xml_Serialization_XmlAttributeAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlAttributeAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0, { ArgumentType: {} argType1 } arg1] when argType0.Equals(typeof(global::System.String)) && argType1.Equals(typeof(global::System.Type)) => new((global::System.String)arg0.Value, (global::System.Type)arg1.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAttributeAttribute.Type) && named.TypedValue is { Value: global::System.Type named_Type })
            {
                attr.Type = named_Type;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAttributeAttribute.AttributeName) && named.TypedValue is { Value: global::System.String named_AttributeName })
            {
                attr.AttributeName = named_AttributeName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAttributeAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAttributeAttribute.DataType) && named.TypedValue is { Value: global::System.String named_DataType })
            {
                attr.DataType = named_DataType;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlAttributeAttribute.Form) && named.TypedValue is { Value: global::System.Xml.Schema.XmlSchemaForm named_Form })
            {
                attr.Form = named_Form;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlChoiceIdentifierAttribute? CreateSystem_Xml_Serialization_XmlChoiceIdentifierAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlChoiceIdentifierAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlChoiceIdentifierAttribute.MemberName) && named.TypedValue is { Value: global::System.String named_MemberName })
            {
                attr.MemberName = named_MemberName;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlElementAttribute? CreateSystem_Xml_Serialization_XmlElementAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlElementAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            [{ ArgumentType: {} argType0 } arg0, { ArgumentType: {} argType1 } arg1] when argType0.Equals(typeof(global::System.String)) && argType1.Equals(typeof(global::System.Type)) => new((global::System.String)arg0.Value, (global::System.Type)arg1.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.Type) && named.TypedValue is { Value: global::System.Type named_Type })
            {
                attr.Type = named_Type;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.ElementName) && named.TypedValue is { Value: global::System.String named_ElementName })
            {
                attr.ElementName = named_ElementName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.DataType) && named.TypedValue is { Value: global::System.String named_DataType })
            {
                attr.DataType = named_DataType;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.IsNullable) && named.TypedValue is { Value: global::System.Boolean named_IsNullable })
            {
                attr.IsNullable = named_IsNullable;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.Form) && named.TypedValue is { Value: global::System.Xml.Schema.XmlSchemaForm named_Form })
            {
                attr.Form = named_Form;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlElementAttribute.Order) && named.TypedValue is { Value: global::System.Int32 named_Order })
            {
                attr.Order = named_Order;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlIgnoreAttribute? CreateSystem_Xml_Serialization_XmlIgnoreAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlIgnoreAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlIncludeAttribute? CreateSystem_Xml_Serialization_XmlIncludeAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlIncludeAttribute? attr = data.ConstructorArguments switch
        {
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlIncludeAttribute.Type) && named.TypedValue is { Value: global::System.Type named_Type })
            {
                attr.Type = named_Type;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlRootAttribute? CreateSystem_Xml_Serialization_XmlRootAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlRootAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlRootAttribute.ElementName) && named.TypedValue is { Value: global::System.String named_ElementName })
            {
                attr.ElementName = named_ElementName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlRootAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlRootAttribute.DataType) && named.TypedValue is { Value: global::System.String named_DataType })
            {
                attr.DataType = named_DataType;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlRootAttribute.IsNullable) && named.TypedValue is { Value: global::System.Boolean named_IsNullable })
            {
                attr.IsNullable = named_IsNullable;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlSchemaProviderAttribute? CreateSystem_Xml_Serialization_XmlSchemaProviderAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlSchemaProviderAttribute? attr = data.ConstructorArguments switch
        {
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlSchemaProviderAttribute.IsAny) && named.TypedValue is { Value: global::System.Boolean named_IsAny })
            {
                attr.IsAny = named_IsAny;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlTextAttribute? CreateSystem_Xml_Serialization_XmlTextAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlTextAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.Type)) => new((global::System.Type)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTextAttribute.Type) && named.TypedValue is { Value: global::System.Type named_Type })
            {
                attr.Type = named_Type;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTextAttribute.DataType) && named.TypedValue is { Value: global::System.String named_DataType })
            {
                attr.DataType = named_DataType;
            }
        }

        return attr;
    }

    private static global::System.Xml.Serialization.XmlTypeAttribute? CreateSystem_Xml_Serialization_XmlTypeAttribute(MetadataLoadContext context, CustomAttributeData data)
    {
        global::System.Xml.Serialization.XmlTypeAttribute? attr = data.ConstructorArguments switch
        {
            [] => new(),
            [{ ArgumentType: {} argType0 } arg0] when argType0.Equals(typeof(global::System.String)) => new((global::System.String)arg0.Value),
            _ => null,
        };

        if (attr is null)
        {
            return null;
        }

        foreach (var named in data.NamedArguments)
        {
            if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTypeAttribute.AnonymousType) && named.TypedValue is { Value: global::System.Boolean named_AnonymousType })
            {
                attr.AnonymousType = named_AnonymousType;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTypeAttribute.IncludeInSchema) && named.TypedValue is { Value: global::System.Boolean named_IncludeInSchema })
            {
                attr.IncludeInSchema = named_IncludeInSchema;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTypeAttribute.TypeName) && named.TypedValue is { Value: global::System.String named_TypeName })
            {
                attr.TypeName = named_TypeName;
            }
            else if (named.MemberName == nameof(global::System.Xml.Serialization.XmlTypeAttribute.Namespace) && named.TypedValue is { Value: global::System.String named_Namespace })
            {
                attr.Namespace = named_Namespace;
            }
        }

        return attr;
    }
}
