using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class RoslynConstructorInfo : ConstructorInfo
    {
        private readonly IMethodSymbol _ctor;
        private readonly RoslynMetadataLoadContext _metadataLoadContext;

        public RoslynConstructorInfo(IMethodSymbol ctor, RoslynMetadataLoadContext metadataLoadContext)
        {
            _ctor = ctor;
            _metadataLoadContext = metadataLoadContext;
            Attributes = SharedUtilities.GetMethodAttributes(ctor);
        }

        public override Type DeclaringType => _ctor.ContainingType.AsType(_metadataLoadContext);

        public override MethodAttributes Attributes { get; }

        public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

        public override string Name => _ctor.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override bool IsGenericMethod => _ctor.IsGenericMethod;

        public override Type[] GetGenericArguments()
        {
            var typeArguments = new List<Type>();
            foreach (var t in _ctor.TypeArguments)
            {
                typeArguments.Add(t.AsType(_metadataLoadContext));
            }
            return typeArguments.ToArray();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(_ctor, _metadataLoadContext);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _metadataLoadContext.Provider.GetProvider(this).GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _metadataLoadContext.Provider.GetProvider(this).GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _metadataLoadContext.Provider.GetProvider(this).IsDefined(attributeType, inherit);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            List<ParameterInfo> parameters = default;
            foreach (var p in _ctor.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(_metadataLoadContext));
            }
            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
#nullable restore
