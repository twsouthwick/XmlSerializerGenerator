using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class RoslynModule : Module
    {
        private readonly Assembly  _assembly;

        public RoslynModule(RoslynAssembly assembly)
        {
            _assembly = assembly;
        }

        public override Assembly Assembly => _assembly;
    }

    internal class RoslynAssembly : Assembly
    {
        private readonly RoslynMetadataLoadContext _metadataLoadContext;

        public RoslynAssembly(IAssemblySymbol assembly, RoslynMetadataLoadContext metadataLoadContext)
        {
            Symbol = assembly;
            _metadataLoadContext = metadataLoadContext;
        }

        public override string FullName => Symbol.Name;

        internal IAssemblySymbol Symbol { get; }

        public override Type[] GetExportedTypes()
        {
            return GetTypes();
        }

        public override Type[] GetTypes()
        {
            var types = new List<Type>();
            var stack = new Stack<INamespaceSymbol>();
            stack.Push(Symbol.GlobalNamespace);
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                foreach (var type in current.GetTypeMembers())
                {
                    types.Add(type.AsType(_metadataLoadContext));
                }

                foreach (var ns in current.GetNamespaceMembers())
                {
                    stack.Push(ns);
                }
            }
            return types.ToArray();
        }

        public override Type GetType(string name)
        {
            return Symbol.GetTypeByMetadataName(name).AsType(_metadataLoadContext);
        }
    }
}
#nullable restore
