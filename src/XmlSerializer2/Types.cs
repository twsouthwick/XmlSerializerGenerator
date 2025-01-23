using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xml.Serialization;

internal static class UrtTypes
{
    internal const string Namespace = "http://microsoft.com/wsdl/types/";
}

internal static class Soap
{
    internal const string Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
    internal const string UrType = "anyType";
    internal const string Array = "Array";
    internal const string ArrayType = "arrayType";
}

internal static class Soap12
{
    internal const string Encoding = "http://www.w3.org/2003/05/soap-encoding";
    internal const string RpcNamespace = "http://www.w3.org/2003/05/soap-rpc";
    internal const string RpcResult = "result";
}

internal static class Wsdl
{
    internal const string Namespace = "http://schemas.xmlsoap.org/wsdl/";
    internal const string ArrayType = "arrayType";
}
