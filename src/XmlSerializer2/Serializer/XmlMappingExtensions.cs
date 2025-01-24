using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Schema;
using Roslyn.Reflection;

namespace System.Xml.Serialization;

internal static class XmlMappingExtensions
{
    internal static readonly BindingFlags Flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    public static bool IsSoap(this XmlMembersMapping mapping) => false;

    public static SpecifiedAccessor CheckSpecified(this MemberMapping mapping) => mapping.CheckSpecified;

    public static XmlQualifiedName GetDerivedFrom(this XmlSchemaType type)
    {
        return type switch
        {
            XmlSchemaComplexType { ContentModel.Content: XmlSchemaComplexContentRestriction r } => r.BaseTypeName,
            XmlSchemaComplexType { ContentModel.Content: XmlSchemaComplexContentExtension e } => e.BaseTypeName,
            XmlSchemaComplexType { ContentModel.Content: XmlSchemaSimpleContentRestriction r } => r.BaseTypeName,
            XmlSchemaComplexType { ContentModel.Content: XmlSchemaSimpleContentExtension e } => e.BaseTypeName,
            XmlSchemaSimpleType { Content: XmlSchemaSimpleTypeRestriction r } => r.BaseTypeName,
            _ => XmlQualifiedName.Empty,
        };
    }

    public static string GetKey(this XmlRootAttribute attr)
    {
        var key = typeof(XmlRootAttribute).GetProperty("Key", Flags);
        return (string)key.GetValue(attr);
    }

    public static XmlSchemaType? Redefined(this XmlSchemaType type)
    {
        // TODO: what should this be?
        return null;
    }

    public static bool GetIsNullableSpecified(this XmlArrayItemAttribute attr)
        => InnerGetIsNullableSpecified(attr);

    public static bool GetIsNullableSpecified(this XmlElementAttribute attr)
        => InnerGetIsNullableSpecified(attr);

    public static bool GetIsNullableSpecified(this XmlRootAttribute attr)
        => InnerGetIsNullableSpecified(attr);

    private static bool InnerGetIsNullableSpecified<T>(T attr)
    {
        var property = typeof(T).GetProperty("IsNullableSpecified", Flags);
        return (bool)property.GetValue(attr);
    }

    public static bool GetNamespaceSpecified(this XmlAnyElementAttribute any)
    {
        var property = typeof(XmlAnyElementAttribute).GetProperty("NamespaceSpecified", Flags);
        return (bool)property.GetValue(any);
    }

    internal static XmlAttributeFlags GetXmlFlags(this XmlAttributes attrs)
    {
        XmlAttributeFlags flags = 0;
        if (attrs.XmlElements.Count > 0) flags |= XmlAttributeFlags.Elements;
        if (attrs.XmlArrayItems.Count > 0) flags |= XmlAttributeFlags.ArrayItems;
        if (attrs.XmlAnyElements.Count > 0) flags |= XmlAttributeFlags.AnyElements;
        if (attrs.XmlArray != null) flags |= XmlAttributeFlags.Array;
        if (attrs.XmlAttribute != null) flags |= XmlAttributeFlags.Attribute;
        if (attrs.XmlText != null) flags |= XmlAttributeFlags.Text;
        if (attrs.XmlEnum != null) flags |= XmlAttributeFlags.Enum;
        if (attrs.XmlRoot != null) flags |= XmlAttributeFlags.Root;
        if (attrs.XmlType != null) flags |= XmlAttributeFlags.Type;
        if (attrs.XmlAnyAttribute != null) flags |= XmlAttributeFlags.AnyAttribute;
        if (attrs.XmlChoiceIdentifier != null) flags |= XmlAttributeFlags.ChoiceIdentifier;
        if (attrs.Xmlns) flags |= XmlAttributeFlags.XmlnsDeclarations;
        return flags;
    }

    internal static MemberInfo GetMemberInfo(this XmlChoiceIdentifierAttribute attr)
    {
        throw new NotImplementedException();
    }

    internal static MemberInfo SetMemberInfo(this XmlChoiceIdentifierAttribute attr, MemberInfo? m)
    {
        throw new NotImplementedException();
    }


    internal static bool TryLookupNamespace(this XmlSerializerNamespaces n, string? prefix, out string? ns)
    {
        var lookupPrefix = typeof(XmlSerializerNamespaces).GetMethod("LookupPrefix", Flags);

        if (lookupPrefix is { })
        {
            ns = (string?)lookupPrefix.Invoke(n, [prefix]);
            return !string.IsNullOrEmpty(ns);
        }

        var tryLookup = typeof(XmlSerializerNamespaces).GetMethod("TryLookupNamespace", Flags);

        var parameters = new object?[] { prefix, null };
        var result = (bool)tryLookup.Invoke(n, parameters);

        ns = result ? (string?)parameters[1] : null;

        return result;
    }
}
