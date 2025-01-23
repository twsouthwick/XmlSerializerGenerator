using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace XmlSerializer2
{
    class DynamicAssemblies
    {
        public static bool IsTypeDynamic(Type type) => false;

        public static bool IsTypeDynamic(Type[] type) => false;

        public static string GetName(Assembly assembly) => assembly.FullName;
    }
}
