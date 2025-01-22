using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xml.Serialization;

internal class XmlMembersMapping2 : XmlMapping2
{
}

internal class XmlTypeMapping2 : XmlMapping2
{
    //
    // Summary:
    //     The fully qualified type name that includes the namespace (or namespaces) and
    //     type.
    //
    // Returns:
    //     The fully qualified type name.
    public string TypeFullName { get; set; } = null!;
    //
    // Summary:
    //     Gets the type name of the mapped object.
    //
    // Returns:
    //     The type name of the mapped object.
    public string TypeName { get; set; } = null!;
    //
    // Summary:
    //     Gets the XML element name of the mapped object.
    //
    // Returns:
    //     The XML element name of the mapped object. The default is the class name of the
    //     object.
    public string XsdTypeName { get; set; } = null!;
    //
    // Summary:
    //     Gets the XML namespace of the mapped object.
    //
    // Returns:
    //     The XML namespace of the mapped object. The default is an empty string ("").
    public string XsdTypeNamespace { get; set; } = null!;
}

internal class XmlMapping2
{
    //
    // Summary:
    //     Get the name of the mapped element.
    //
    // Returns:
    //     The name of the mapped element.
    public string ElementName { get; } = null!;
    //
    // Summary:
    //     Gets the namespace of the mapped element.
    //
    // Returns:
    //     The namespace of the mapped element.
    public string Namespace { get; } = null!;
    //
    // Summary:
    //     Gets the name of the XSD element of the mapping.
    //
    // Returns:
    //     The XSD element name.
    public string XsdElementName { get; } = null!;

    //
    // Summary:
    //     Sets the key used to look up the mapping.
    //
    // Parameters:
    //   key:
    //     A System.String that contains the lookup key.
    public void SetKey(string key)
    {
        Key = key;
    }

    public string? Key { get; set; }

    public required Accessor Accessor { get; set; }
}

internal class TypeScope
{
    public TypeDesc GetTypeDesc(Type type)
    {
        throw new NotImplementedException();
    }
}

internal class TypeDesc : ITypeDescType
{
    public string? FullName { get; set; }

    public string Name { get; set; } = null!;

    public int Weight { get; set; }

    public bool IsRoot { get; set; }

    public bool CanBeTextValue { get; set; }

    public bool IsEnum { get; set; }

    public bool IsPrimitive { get; set; }

    public bool CanBeElementValue { get; set; }

    public ITypeDescType Type => this;

    public bool IsPublic { get; set; }

    public bool IsNestedPublic { get; set; }

    public bool IsGenericType { get; set; }

    public string? Namespace { get; }

    public ITypeDescType GetElementType() => throw new NotImplementedException();

    public bool IsArray { get; }

    public bool ContainsGenericParameters { get; }

    public ITypeDescType[] GetGenericArguments() => throw new NotImplementedException();

    public required ITypeDescType DeclaringType { get; init; }
}

internal interface ITypeDescType
{
    bool IsPublic { get; }

    bool IsNestedPublic { get; }

    bool IsGenericType { get; }

    string? Namespace { get; }

    ITypeDescType GetElementType();

    bool ContainsGenericParameters { get; }

    ITypeDescType[] GetGenericArguments();

    bool IsArray { get; }

    ITypeDescType DeclaringType { get; }

    string Name { get; }
}

internal static class XmlReservedNs
{
    internal const string NsXml = "http://www.w3.org/XML/1998/namespace";
    internal const string NsXmlNs = "http://www.w3.org/2000/xmlns/";
    internal const string NsDataType = "urn:schemas-microsoft-com:datatypes";
    internal const string NsDataTypeAlias = "uuid:C2F41010-65B3-11D1-A29F-00AA00C14882";
    internal const string NsDataTypeOld = "urn:uuid:C2F41010-65B3-11D1-A29F-00AA00C14882/";
    internal const string NsMsxsl = "urn:schemas-microsoft-com:xslt";
    internal const string NsXdr = "urn:schemas-microsoft-com:xml-data";
    internal const string NsXslDebug = "urn:schemas-microsoft-com:xslt-debug";
    internal const string NsXdrAlias = "uuid:BDC6E3F0-6DA3-11D1-A2A3-00AA00C14882";
    internal const string NsWdXsl = "http://www.w3.org/TR/WD-xsl";
    internal const string NsXs = "http://www.w3.org/2001/XMLSchema";
    internal const string NsXsi = "http://www.w3.org/2001/XMLSchema-instance";
    internal const string NsXslt = "http://www.w3.org/1999/XSL/Transform";
    internal const string NsExsltCommon = "http://exslt.org/common";
    internal const string NsXQueryDataType = "http://www.w3.org/2003/11/xpath-datatypes";
    internal const string NsCollationBase = "http://collations.microsoft.com";
    internal const string NsCollCodePoint = "http://www.w3.org/2004/10/xpath-functions/collation/codepoint";
};
