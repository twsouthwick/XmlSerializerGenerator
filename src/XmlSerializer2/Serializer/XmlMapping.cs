using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Xml.Serialization;

///<internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
internal abstract class XmlMapping
{
    private readonly TypeScope? _scope;
    private bool _generateSerializer;
    private bool _isSoap;
    private readonly ElementAccessor _accessor;
    private string? _key;
    private readonly bool _shallow;
    private readonly XmlMappingAccess _access;

    internal XmlMapping(TypeScope? scope, ElementAccessor accessor) : this(scope, accessor, XmlMappingAccess.Read | XmlMappingAccess.Write)
    {
    }

    internal XmlMapping(TypeScope? scope, ElementAccessor accessor, XmlMappingAccess access)
    {
        _scope = scope;
        _accessor = accessor;
        _access = access;
        _shallow = scope == null;
    }

    internal ElementAccessor Accessor
    {
        get { return _accessor; }
    }

    internal TypeScope? Scope
    {
        get { return _scope; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string ElementName
    {
        get { return System.Xml.Serialization.Accessor.UnescapeName(Accessor.Name); }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string XsdElementName
    {
        get { return Accessor.Name; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? Namespace
    {
        get { return _accessor.Namespace; }
    }

    internal bool GenerateSerializer
    {
        get { return _generateSerializer; }
        set { _generateSerializer = value; }
    }

    internal bool IsReadable
    {
        get { return ((_access & XmlMappingAccess.Read) != 0); }
    }

    internal bool IsWriteable
    {
        get { return ((_access & XmlMappingAccess.Write) != 0); }
    }

    internal bool IsSoap
    {
        get { return _isSoap; }
        set { _isSoap = value; }
    }

    ///<internalonly/>
    public void SetKey(string? key)
    {
        SetKeyInternal(key);
    }

    ///<internalonly/>
    internal void SetKeyInternal(string? key)
    {
        _key = key;
    }

    internal static string GenerateKey(Type type, XmlRootAttribute? root, string? ns)
    {
        MemberInfo m = type;
        root ??= (XmlRootAttribute)type.GetCustomAttributes(typeof(XmlRootAttribute), false).FirstOrDefault();
        return $"{type.FullName}:{(root == null ? string.Empty : root.GetKey())}:{ns ?? string.Empty}";
    }

    internal string? Key { get { return _key; } }

    internal void CheckShallow()
    {
        if (_shallow)
        {
            throw new InvalidOperationException(SR.XmlMelformMapping);
        }
    }

    internal static bool IsShallow(List<XmlMapping> mappings)
    {
        for (int i = 0; i < mappings.Count; i++)
        {
            if (mappings[i] == null || mappings[i]._shallow)
                return true;
        }
        return false;
    }
}

internal class XmlTypeMapping : XmlMapping
{
    internal XmlTypeMapping(TypeScope? scope, ElementAccessor accessor) : base(scope, accessor)
    {
    }

    internal TypeMapping? Mapping
    {
        get { return Accessor.Mapping; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string TypeName
    {
        get
        {
            return Mapping!.TypeDesc!.Name;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string TypeFullName
    {
        get
        {
            return Mapping!.TypeDesc!.FullName;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? XsdTypeName
    {
        get
        {
            return Mapping!.TypeName;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? XsdTypeNamespace
    {
        get
        {
            return Mapping!.Namespace;
        }
    }
}

internal class XmlMembersMapping : XmlMapping
{
    private readonly XmlMemberMapping[] _mappings;

    internal XmlMembersMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access) : base(scope, accessor, access)
    {
        MembersMapping mapping = (MembersMapping)accessor.Mapping!;
        StringBuilder key = new StringBuilder();
        key.Append(':');
        _mappings = new XmlMemberMapping[mapping.Members!.Length];
        for (int i = 0; i < _mappings.Length; i++)
        {
            if (mapping.Members[i].TypeDesc!.Type != null)
            {
                key.Append(GenerateKey(mapping.Members[i].TypeDesc!.Type!, null, null));
                key.Append(':');
            }
            _mappings[i] = new XmlMemberMapping(mapping.Members[i]);
        }
        SetKeyInternal(key.ToString());
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? TypeName
    {
        get { return Accessor.Mapping!.TypeName; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? TypeNamespace
    {
        get { return Accessor.Mapping!.Namespace; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public XmlMemberMapping this[int index]
    {
        get { return _mappings[index]; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public int Count
    {
        get { return _mappings.Length; }
    }
}
class XmlMemberMapping
{
    private readonly MemberMapping _mapping;

    internal XmlMemberMapping(MemberMapping mapping)
    {
        _mapping = mapping;
    }

    internal MemberMapping Mapping
    {
        get { return _mapping; }
    }

    internal Accessor? Accessor
    {
        get { return _mapping.Accessor; }
    }

    public bool Any
    {
        get { return Accessor!.Any; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string ElementName
    {
        get { return Accessor.UnescapeName(Accessor!.Name); }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string XsdElementName
    {
        get { return Accessor!.Name; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? Namespace
    {
        get { return Accessor!.Namespace; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string MemberName
    {
        get { return _mapping.Name; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? TypeName
    {
        get { return Accessor!.Mapping != null ? Accessor.Mapping.TypeName : string.Empty; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string? TypeNamespace
    {
        get { return Accessor!.Mapping != null ? Accessor.Mapping.Namespace : null; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string TypeFullName
    {
        get { return _mapping.TypeDesc!.FullName; }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public bool CheckSpecified
    {
        get { return _mapping.CheckSpecified != SpecifiedAccessor.None; }
    }
}

