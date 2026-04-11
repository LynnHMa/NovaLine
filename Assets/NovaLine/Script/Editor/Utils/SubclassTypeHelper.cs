using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaLine.Script.Editor.Utils
{
    public static class SubclassTypeHelper
    {
        private static readonly Dictionary<Type, List<Type>> cache = new();

        public static List<Type> GetSubTypes(Type baseType)
        {
            if (cache.TryGetValue(baseType, out var list))
                return list;

            list = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t =>
                    baseType.IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface)
                .ToList();

            cache[baseType] = list;
            return list;
        }
    }
}