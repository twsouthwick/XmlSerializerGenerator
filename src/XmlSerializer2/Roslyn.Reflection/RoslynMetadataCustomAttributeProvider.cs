using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using XmlSerializer2;

#nullable disable
namespace Roslyn.Reflection;

internal sealed class RoslynMetadataCustomAttributeProvider(RoslynMetadataLoadContext ctx)
{
    private IEnumerable<object> Create(IEnumerable<CustomAttributeData> data)
    {
        foreach (var attr in data)
        {
            if (AttributeProviders.Create(ctx, attr) is { } obj)
            {
                yield return obj;
            }
        }
    }

    private IEnumerable<object> GetCustomAttributes(MemberInfo member, bool inherit)
    {
        var queue = new Queue<MemberInfo>();
        queue.Enqueue(member);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var data = current.GetCustomAttributesData();

            foreach (var attr in Create(data))
            {
                yield return attr;
            }

            if (current is Type t)
            {
                queue.Enqueue(t.BaseType);

                foreach (var i in t.GetInterfaces())
                {
                    queue.Enqueue(i);
                }
            }
        }
    }

    private IEnumerable<object> GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(member, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }

    private bool IsDefined(MemberInfo member, Type attributeType, bool inherit)
        => GetCustomAttributes(member, attributeType, inherit).Any();

    private IEnumerable<object> GetCustomAttributes(Module module, bool inherit)
    {
        // TODO handle inherit
        return Create(module.GetCustomAttributesData());
    }

    private IEnumerable<object> GetCustomAttributes(Module module, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(module, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }

    private bool IsDefined(Module module, Type attributeType, bool inherit)
         => GetCustomAttributes(module, attributeType, inherit).Any();

    private IEnumerable<object> GetCustomAttributes(Assembly assembly, bool inherit)
    {
        // TODO handle inherit
        return Create(assembly.GetCustomAttributesData());
    }

    private IEnumerable<object> GetCustomAttributes(Assembly assembly, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(assembly, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }

    private bool IsDefined(Assembly assembly, Type attributeType, bool inherit)
        => GetCustomAttributes(assembly, attributeType, inherit).Any();

    internal ICustomAttributeProvider GetProvider(MemberInfo member) => new MemberInfoProvider(member, this);

    internal ICustomAttributeProvider GetProvider(Module module) => new ModuleProvider(module, this);

    internal ICustomAttributeProvider GetProvider(Assembly assembly) => new AssemblyProvider(assembly, this);

    private sealed class MemberInfoProvider(MemberInfo member, RoslynMetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(member, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(member, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(member, attributeType, inherit);
    }

    private sealed class ModuleProvider(Module module, RoslynMetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(module, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(module, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(module, attributeType, inherit);
    }

    private sealed class AssemblyProvider(Assembly assembly, RoslynMetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(assembly, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(assembly, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(assembly, attributeType, inherit);
    }
}
#nullable restore
