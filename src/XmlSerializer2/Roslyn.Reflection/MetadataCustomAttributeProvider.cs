using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection;

public abstract class MetadataCustomAttributeProvider
{
    public static readonly MetadataCustomAttributeProvider NotSupported = new NotSupportedProvider();

    protected abstract IEnumerable<object> GetCustomAttributes(MemberInfo member, bool inherit);
    
    protected virtual IEnumerable<object> GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(member, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }
    protected virtual bool IsDefined(MemberInfo member, Type attributeType, bool inherit)
        => GetCustomAttributes(member, attributeType, inherit).Any();
    protected abstract IEnumerable<object> GetCustomAttributes(Module module, bool inherit);
    protected virtual IEnumerable<object> GetCustomAttributes(Module module, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(module, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }
    protected virtual bool IsDefined(Module module, Type attributeType, bool inherit)
        => GetCustomAttributes(module, attributeType, inherit).Any();
    protected abstract IEnumerable<object> GetCustomAttributes(Assembly assembly, bool inherit);
    protected virtual IEnumerable<object> GetCustomAttributes(Assembly assembly, Type attributeType, bool inherit)
    {
        foreach (var attr in GetCustomAttributes(assembly, inherit))
        {
            if (attr.GetType() == attributeType)
            {
                yield return attr;
            }
        }
    }
    protected virtual bool IsDefined(Assembly assembly, Type attributeType, bool inherit)
        => GetCustomAttributes(assembly, attributeType, inherit).Any();
    internal ICustomAttributeProvider GetProvider(MemberInfo member) => new MemberInfoProvider(member, this);
    internal ICustomAttributeProvider GetProvider(Module module) => new ModuleProvider(module, this);
    internal ICustomAttributeProvider GetProvider(Assembly assembly) => new AssemblyProvider(assembly, this);
    private sealed class MemberInfoProvider(MemberInfo member, MetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(member, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(member, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(member, attributeType, inherit);
    }
    private sealed class ModuleProvider(Module module, MetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(module, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(module, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(module, attributeType, inherit);
    }
    private sealed class AssemblyProvider(Assembly assembly, MetadataCustomAttributeProvider provider) : ICustomAttributeProvider
    {
        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) => provider.GetCustomAttributes(assembly, inherit).ToArray();
        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) => provider.GetCustomAttributes(assembly, attributeType, inherit).ToArray();
        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) => provider.IsDefined(assembly, attributeType, inherit);
    }
    private sealed class NotSupportedProvider : MetadataCustomAttributeProvider
    {
        protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, bool inherit) => throw new InvalidOperationException(SR.Arg_ReflectionOnlyCA);
        protected override IEnumerable<object> GetCustomAttributes(Module module, bool inherit) => throw new InvalidOperationException(SR.Arg_ReflectionOnlyCA);
        protected override IEnumerable<object> GetCustomAttributes(Assembly assembly, bool inherit) => throw new InvalidOperationException(SR.Arg_ReflectionOnlyCA);
    }
}
#nullable restore
