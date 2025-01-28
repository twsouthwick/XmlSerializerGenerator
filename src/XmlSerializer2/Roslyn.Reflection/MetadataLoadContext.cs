using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using XmlSerializer2;

#nullable disable
namespace Roslyn.Reflection;

public class MetadataLoadContext
{
    private readonly Compilation _compilation;
    private readonly MetadataCustomAttributeProvider _provider;

    private readonly ConcurrentDictionary<ISymbol, object> _cache = new(SymbolEqualityComparer.Default);

    public MetadataLoadContext(Compilation compilation)
    {
        _compilation = compilation;
        _provider = new(this);
    }

    public Assembly Assembly => _compilation.Assembly.AsAssembly(this);

    internal Compilation Compilation => _compilation;

    internal MetadataCustomAttributeProvider Provider => _provider;

    public Type ResolveType(string fullyQualifiedMetadataName)
    {
        return _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)?.AsType(this);
    }

    public Type ResolveType<T>() => ResolveType(typeof(T));

    public Type ResolveType(Type type)
    {
        if (type is null)
        {
            return null;
        }

        var resolvedType = _compilation.GetTypeByMetadataName(type.FullName);

        if (resolvedType is not null)
        {
            return resolvedType.AsType(this);
        }

        if (type.IsArray)
        {
            var typeSymbol = _compilation.GetTypeByMetadataName(type.GetElementType().FullName);
            if (typeSymbol is null)
            {
                return null;
            }

            return _compilation.CreateArrayTypeSymbol(typeSymbol).AsType(this);
        }

        if (type.IsGenericType)
        {
            var openGenericTypeSymbol = _compilation.GetTypeByMetadataName(type.GetGenericTypeDefinition().FullName);
            if (openGenericTypeSymbol is null)
            {
                return null;
            }

            return openGenericTypeSymbol.AsType(this).MakeGenericType(type.GetGenericArguments());
        }

        return null;
    }

    public TMember GetOrCreate<TMember>(ISymbol symbol) where TMember : class
    {
        if (symbol is null)
        {
            return null;
        }

        // For now, take the first one
        if (symbol is IErrorTypeSymbol error)
        {
            symbol = error.CandidateSymbols.FirstOrDefault();
        }

        return (TMember)_cache.GetOrAdd(symbol, s => s switch
        {
            ITypeSymbol t => new RoslynType(t, this),
            IFieldSymbol f => new RoslynFieldInfo(f, this),
            IPropertySymbol p => new RoslynPropertyInfo(p, this),
            IMethodSymbol c when c.MethodKind == MethodKind.Constructor => new RoslynConstructorInfo(c, this),
            IMethodSymbol m => new RoslynMethodInfo(m, this),
            IParameterSymbol param => new RoslynParameterInfo(param, this),
            IAssemblySymbol a => new RoslynAssembly(a, this),
            _ => null
        });
    }

    public TMember ResolveMember<TMember>(TMember memberInfo) where TMember : MemberInfo
    {
        return memberInfo switch
        {
            RoslynFieldInfo f => (TMember)(object)f,
            RoslynMethodInfo m => (TMember)(object)m,
            RoslynPropertyInfo p => (TMember)(object)p,
            MethodInfo m => (TMember)(object)ResolveType(m.ReflectedType)?.GetMethod(m.Name, SharedUtilities.ComputeBindingFlags(m), binder: null, types: m.GetParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
            PropertyInfo p => (TMember)(object)ResolveType(p.ReflectedType)?.GetProperty(p.Name, SharedUtilities.ComputeBindingFlags(p), binder: null, returnType: p.PropertyType, types: p.GetIndexParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
            FieldInfo f => (TMember)(object)ResolveType(f.ReflectedType)?.GetField(f.Name, SharedUtilities.ComputeBindingFlags(f)),
            _ => null
        };
    }

    internal sealed class MetadataCustomAttributeProvider(MetadataLoadContext ctx)
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
            // TODO handle inherit
            var data = member.GetCustomAttributesData();
            return Create(data);
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
    }
}
#nullable restore
