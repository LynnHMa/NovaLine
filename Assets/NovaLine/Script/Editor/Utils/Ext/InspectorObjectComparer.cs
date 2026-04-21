using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils.Ext
{
    public static class InspectorObjectComparer
    {
        private static readonly Dictionary<Type, FieldInfo[]> _cache = new();

        public static bool InspectorEquals<T>(this T obj1, T obj2)
        {
            return EqualsInternal(obj1, obj2, new HashSet<object>());
        }

        private static bool EqualsInternal(object obj1, object obj2, HashSet<object> visited)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 == null || obj2 == null) return false;

            var type = obj1.GetType();
            if (type != obj2.GetType()) return false;
            
            if (obj1 is UnityEngine.Object u1 && obj2 is UnityEngine.Object u2)
            {
                return u1 == u2;
            }
            
            if (!type.IsValueType)
            {
                if (!visited.Add(obj1)) return true;
            }
            
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
                return obj1.Equals(obj2);
            
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var e1 = ((IEnumerable)obj1).GetEnumerator();
                using var e3 = e1 as IDisposable;
                var e2 = ((IEnumerable)obj2).GetEnumerator();
                using var e4 = e2 as IDisposable;

                while (true)
                {
                    bool m1 = e1.MoveNext();
                    bool m2 = e2.MoveNext();

                    if (m1 != m2) return false;
                    if (!m1) break;

                    if (!EqualsInternal(e1.Current, e2.Current, visited))
                        return false;
                }

                return true;
            }
            
            foreach (var field in GetUnitySerializableFields(type))
            {
                var v1 = field.GetValue(obj1);
                var v2 = field.GetValue(obj2);

                if (!EqualsInternal(v1, v2, visited))
                    return false;
            }

            return true;
        }

        private static FieldInfo[] GetUnitySerializableFields(Type type)
        {
            if (_cache.TryGetValue(type, out var fields))
                return fields;

            var all = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var list = new List<FieldInfo>();

            foreach (var f in all)
            {
                if (f.IsStatic) continue;
                if (f.IsDefined(typeof(NonSerializedAttribute), true)) continue;

                bool isPublic = f.IsPublic;
                bool hasSerializeField = f.IsDefined(typeof(SerializeField), true);

                if (isPublic || hasSerializeField)
                {
                    list.Add(f);
                }
            }

            fields = list.ToArray();
            _cache[type] = fields;
            return fields;
        }
    }
}