using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace XmlSerializer2;

internal static class XmlMappingExtensions
{
    internal static readonly BindingFlags Flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    public static bool IsSoap(this XmlMembersMapping mapping) => false;

    public static SpecifiedAccessor CheckSpecified(this MemberMapping mapping) => SpecifiedAccessor.None;

    public static ElementAccessor Accessor(this XmlMapping mapping)
    {
        var accessor = typeof(XmlMapping).GetMethod("Accessor", Flags);
        return (ElementAccessor)accessor.Invoke(mapping, []);
    }

    public static string GetKey(this XmlMapping mapping)
    {
        var key = typeof(XmlMapping).GetProperty("Key", Flags);
        return (string)key.GetValue(mapping);
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
