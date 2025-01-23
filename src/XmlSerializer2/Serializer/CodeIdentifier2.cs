using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace XmlSerializer2;

internal class CodeIdentifier2
{
    internal const int MaxIdentifierLength = 511;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static string MakePascal(string identifier)
    {
        return System.Xml.Serialization.CodeIdentifier.MakePascal(identifier);
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static string MakeCamel(string identifier)
    {
        return System.Xml.Serialization.CodeIdentifier.MakeCamel(identifier);
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static string MakeValid(string identifier)
    {
        return System.Xml.Serialization.CodeIdentifier.MakeValid(identifier);
    }

    internal static string MakeValidInternal(string identifier)
    {
        if (identifier.Length > 30)
        {
            return "Item";
        }
        return MakeValid(identifier);
    }

    internal static void CheckValidIdentifier(string? ident)
    {
        // TODO IMPLEMENT
#if FALSE
        if (!CSharpHelpers.IsValidLanguageIndependentIdentifier(ident))
            throw new ArgumentException(SR.Format(SR.XmlInvalidIdentifier, ident), nameof(ident));

        Debug.Assert(ident != null);
#endif
    }

    internal static string GetCSharpName(string name)
    {
        return EscapeKeywords(name.Replace('+', '.'));
    }

    private static int GetCSharpName(Type t, Type[] parameters, int index, StringBuilder sb)
    {
        if (t.DeclaringType != null && t.DeclaringType != t)
        {
            index = GetCSharpName(t.DeclaringType, parameters, index, sb);
            sb.Append('.');
        }
        string name = t.Name;
        int nameEnd = name.IndexOf('`');
        if (nameEnd < 0)
        {
            nameEnd = name.IndexOf('!');
        }
        if (nameEnd > 0)
        {
            EscapeKeywords(name.Substring(0, nameEnd), sb);
            sb.Append('<');
            int arguments = int.Parse(name.AsSpan(nameEnd + 1).ToString(), provider: CultureInfo.InvariantCulture) + index;
            for (; index < arguments; index++)
            {
                sb.Append(GetCSharpName(parameters[index]));
                if (index < arguments - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append('>');
        }
        else
        {
            EscapeKeywords(name, sb);
        }
        return index;
    }

    internal static string GetCSharpName(Type t)
    {
        int rank = 0;
        while (t.IsArray)
        {
            t = t.GetElementType()!;
            rank++;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("global::");
        string? ns = t.Namespace;
        if (ns != null && ns.Length > 0)
        {
            string[] parts = ns.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                EscapeKeywords(parts[i], sb);
                sb.Append('.');
            }
        }

        Type[] arguments = t.IsGenericType || t.ContainsGenericParameters ? t.GetGenericArguments() : Type.EmptyTypes;
        GetCSharpName(t, arguments, 0, sb);
        for (int i = 0; i < rank; i++)
        {
            sb.Append("[]");
        }
        return sb.ToString();
    }

    /*
    internal static string GetTypeName(string name, CodeDomProvider codeProvider) {
        return codeProvider.GetTypeOutput(new CodeTypeReference(name));
    }
    */

    private static void EscapeKeywords(string identifier, StringBuilder sb)
    {
        if (string.IsNullOrEmpty(identifier))
            return;
        int arrayCount = 0;
        while (identifier.EndsWith("[]", StringComparison.Ordinal))
        {
            arrayCount++;
            identifier = identifier.Substring(0, identifier.Length - 2);
        }
        if (identifier.Length > 0)
        {
            CheckValidIdentifier(identifier);
            identifier = CSharpHelpers.CreateEscapedIdentifier(identifier);
            sb.Append(identifier);
        }
        for (int i = 0; i < arrayCount; i++)
        {
            sb.Append("[]");
        }
    }

    private static readonly char[] s_identifierSeparators = new char[] { '.', ',', '<', '>' };

    [return: NotNullIfNotNull(nameof(identifier))]
    private static string? EscapeKeywords(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return identifier;
        string originalIdentifier = identifier!;
        string[] names = identifier!.Split(s_identifierSeparators);
        StringBuilder sb = new StringBuilder();
        int separator = -1;
        for (int i = 0; i < names.Length; i++)
        {
            if (separator >= 0)
            {
                sb.Append(originalIdentifier[separator]);
            }
            separator++;
            separator += names[i].Length;
            string escapedName = names[i].Trim();
            EscapeKeywords(escapedName, sb);
        }
        if (sb.Length != originalIdentifier.Length)
            return sb.ToString();
        return originalIdentifier;
    }
}