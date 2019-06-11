using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StrangeFog.Docker.Monitor.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypesHierarchy(this Type type, bool ignoreBaseObjectType = true)
        {
            var current = type;
            while (current != null && (!ignoreBaseObjectType || current != typeof(object)))
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }
}
