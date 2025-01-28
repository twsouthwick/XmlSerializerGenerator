using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using XmlSerializer2;

namespace System.Xml.Serialization;

internal sealed class Compiler
{
    private readonly StringWriter _writer = new StringWriter(CultureInfo.InvariantCulture);

    // SxS: This method does not take any resource name and does not expose any resources to the caller.
    // It's OK to suppress the SxS warning.
    [RequiresUnreferencedCode("Reflects against input Type DeclaringType")]
    internal static void AddImport(Type? type, Hashtable types)
    {
        if (type == null)
            return;
        if (TypeScope.IsKnownType(type))
            return;
        if (types[type] != null)
            return;
        types[type] = type;
        Type? baseType = type.BaseType;
        if (baseType != null)
            AddImport(baseType, types);

        Type? declaringType = type.DeclaringType;
        if (declaringType != null)
            AddImport(declaringType, types);

        foreach (Type intf in type.GetInterfaces())
            AddImport(intf, types);

        ConstructorInfo[] ctors = type.GetConstructors();
        for (int i = 0; i < ctors.Length; i++)
        {
            ParameterInfo[] parms = ctors[i].GetParameters();
            for (int j = 0; j < parms.Length; j++)
            {
                AddImport(parms[j].ParameterType, types);
            }
        }

        if (type.IsGenericType)
        {
            Type[] arguments = type.GetGenericArguments();
            for (int i = 0; i < arguments.Length; i++)
            {
                AddImport(arguments[i], types);
            }
        }

        Module module = type.Module;
        Assembly assembly = module.Assembly;
        if (DynamicAssemblies.IsTypeDynamic(type))
        {
            DynamicAssemblies.Add(assembly);
            return;
        }

        object[] typeForwardedFromAttribute = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
        if (typeForwardedFromAttribute.Length > 0)
        {
            throw new InvalidOperationException("Tried to load assembly");
#if FALSE
            TypeForwardedFromAttribute? originalAssemblyInfo = typeForwardedFromAttribute[0] as TypeForwardedFromAttribute;
            Debug.Assert(originalAssemblyInfo != null);
            Assembly.Load(new AssemblyName(originalAssemblyInfo.AssemblyFullName));
#endif
        }
    }

    internal TextWriter Source
    {
        get { return _writer; }
    }

    internal static string GetTempAssemblyName(AssemblyName parent, string? ns)
    {
        return string.IsNullOrEmpty(ns) ?
            $"{parent.Name}.XmlSerializers" :
            $"{parent.Name}.XmlSerializers.{GetPersistentHashCode(ns)}";
    }

    private static readonly SHA256 _sha256 = SHA256.Create();

    private static uint GetPersistentHashCode(string value)
    {
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hash = _sha256.ComputeHash(valueBytes);
        return BinaryPrimitives.ReadUInt32BigEndian(hash);
    }
}