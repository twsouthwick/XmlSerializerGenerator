// See https://aka.ms/new-console-template for more information
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

var builder = new Builder();

builder.Add<ObsoleteAttribute>(nameof(ObsoleteAttribute.UrlFormat), nameof(ObsoleteAttribute.DiagnosticId));
builder.Add<XmlAnyElementAttribute>();
builder.Add<XmlElementAttribute>();
builder.Add<XmlRootAttribute>();
builder.Add<XmlTextAttribute>();
builder.Add<DataContractAttribute>();
builder.Add<DataMemberAttribute>();
builder.Add<XmlIncludeAttribute>();
builder.Add<XmlAttributeAttribute>();
builder.Add<XmlSchemaProviderAttribute>();
builder.Add<XmlTypeAttribute>();
builder.Add<XmlArrayAttribute>();
builder.Add<XmlChoiceIdentifierAttribute>();
builder.Add<XmlIgnoreAttribute>();
builder.Add<XmlArrayItemAttribute>();
builder.Add<KnownTypeAttribute>();
builder.Add<DefaultMemberAttribute>();

using (var fs = File.OpenWrite(Path.Combine(GetGitDir(), "src", "XmlSerializer2", "AttributeProviders.g.cs")))
{
    fs.SetLength(0);

    using var writer = new StreamWriter(fs);
    builder.Write(writer);
}

static string GetGitDir()
{
    return Inner(AppContext.BaseDirectory);

    static string Inner(string? dir)
    {
        if (dir is null)
        {
            throw new DirectoryNotFoundException("No .git dir found");
        }

        if (Directory.Exists(Path.Combine(dir, ".git")))
        {
            return dir;
        }

        return Inner(Directory.GetParent(dir)?.FullName);
    }
}

class Builder
{
    private readonly SortedList<string, (Type, string[])> _types = [];

    public void Add<T>(params string[] skip)
        where T : Attribute
    {
        Add(typeof(T), skip);
    }

    private void Add(Type type, params string[] skip)
    {
        _types.Add(type.FullName!.Replace(".", "_"), (type, skip));
    }

    public void Write(TextWriter writer)
    {
        var indented = new IndentedTextWriter(writer);

        WriteUsings([
            "System",
            "Roslyn.Reflection",
            "System.Collections.Generic",
            "System.Reflection"
            ], indented);

        indented.WriteLineNoTabs(string.Empty);
        indented.WriteLine("#nullable enable");
        indented.WriteLineNoTabs(string.Empty);
        indented.WriteLine("namespace XmlSerializer2;");
        indented.WriteLineNoTabs(string.Empty);

        indented.WriteLine("internal class AttributeProviders");
        indented.WriteLine("{");
        indented.Indent++;

        WriteDictionary(indented);
        WriteMethods(indented);
        WriteIndented(indented);

        indented.Indent--;
        indented.WriteLine("}");
    }

    private void WriteUsings(IEnumerable<string> usings, IndentedTextWriter writer)
    {
        foreach (var u in usings.OrderBy(u => u))
        {
            writer.Write("using ");
            writer.Write(u);
            writer.WriteLine(";");
        }
    }

    private void WriteDictionary(IndentedTextWriter writer)
    {
        writer.WriteLine("private static readonly Dictionary<string, Func<RoslynMetadataLoadContext, CustomAttributeData, object?>> _map = new(StringComparer.OrdinalIgnoreCase)");
        writer.WriteLine("{");
        writer.Indent++;

        foreach (var (name, (type, _)) in _types)
        {
            writer.Write("{ \"");
            writer.Write(type.FullName);
            writer.Write("\", Create");
            writer.Write(name);
            writer.WriteLine(" },");
        }

        writer.Indent--;
        writer.WriteLine("};");
        writer.WriteLineNoTabs(string.Empty);
    }

    private void WriteMethods(IndentedTextWriter writer)
    {
        writer.WriteLine("""
            public static object? Create(RoslynMetadataLoadContext context, CustomAttributeData data)
            {
                if (_map.TryGetValue(data.AttributeType.FullName, out var func))
                {
                    return func(context, data);
                }

                return null;
            }
            """);
        writer.WriteLineNoTabs(string.Empty);
    }
    private void WriteIndented(IndentedTextWriter writer)
    {
        var count = 0;
        foreach (var (name, (type, skip)) in _types)
        {
            if (count > 0)
            {
                writer.WriteLineNoTabs(string.Empty);
            }
            count++;

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.SetMethod is not null)
                .Where(p => !skip.Contains(p.Name))
                .ToList();
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            writer.Write("private static ");
            WriteFullName(writer, type);
            writer.Write("? Create");
            writer.Write(name);
            writer.Write("(RoslynMetadataLoadContext context, CustomAttributeData data)");
            writer.WriteLine();
            writer.WriteLine("{");
            writer.Indent++;

            WriteFullName(writer, type);
            writer.WriteLine("? attr = data.ConstructorArguments switch");
            writer.WriteLine("{");
            writer.Indent++;

            foreach (var c in constructors)
            {
                writer.Write("[");
                var ps = c.GetParameters();

                for (int i = 0; i < ps.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                    }

                    writer.Write("{ ArgumentType: {} argType");
                    writer.Write(i);
                    writer.Write(" } arg");
                    writer.Write(i);
                }

                writer.Write("]");

                if (ps.Length > 0)
                {
                    writer.Write(" when ");

                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (i > 0)
                        {
                            writer.Write("&& ");
                        }

                        writer.Write("argType");
                        writer.Write(i);
                        writer.Write(".Equals(typeof(");
                        WriteFullName(writer, ps[i].ParameterType);
                        writer.Write(")) ");
                    }
                }
                else
                {
                    writer.Write(" ");
                }

                writer.Write("=> new(");

                for (int i = 0; i < ps.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(", ");
                    }

                    writer.Write("(");
                    WriteFullName(writer, ps[i].ParameterType);
                    writer.Write(")arg");
                    writer.Write(i);
                    writer.Write(".Value");
                }

                writer.WriteLine("),");
            }

            writer.WriteLine("_ => null,");

            writer.Indent--;
            writer.WriteLine("};");
            writer.WriteLineNoTabs(string.Empty);

            writer.WriteLine("if (attr is null)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("return null;");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLineNoTabs(string.Empty);

            if (properties.Count > 0)
            {
                writer.WriteLine("foreach (var named in data.NamedArguments)");
                writer.WriteLine("{");
                writer.Indent++;

                var includeElse = false;
                foreach (var property in properties)
                {
                    if (property.SetMethod is { })
                    {
                        if (includeElse)
                        {
                            writer.Write("else ");
                        }
                        else
                        {
                            includeElse = true;
                        }

                        writer.Write("if (named.MemberName == nameof(");
                        WriteFullName(writer, type);
                        writer.Write(".");
                        writer.Write(property.Name);
                        writer.Write(") && named.TypedValue is { Value: ");
                        WriteFullName(writer, property.PropertyType);
                        writer.Write(" named_");
                        writer.Write(property.Name);
                        writer.WriteLine(" })");
                        writer.WriteLine("{");
                        writer.Indent++;
                        writer.Write("attr.");
                        writer.Write(property.Name);
                        writer.Write(" = named_");
                        writer.Write(property.Name);
                        writer.WriteLine(";");
                        writer.Indent--;
                        writer.WriteLine("}");
                    }
                }

                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLineNoTabs(string.Empty);
            }

            writer.WriteLine("return attr;");

            writer.Indent--;
            writer.WriteLine("}");
        }
    }

    private void WriteFullName(TextWriter writer, Type type)
    {
        writer.Write("global::");
        writer.Write(type.FullName);
    }
}
