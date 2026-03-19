using NovaLine.Utils.Interface;
using System.Collections.Generic;

namespace NovaLine.Utils.Ext
{
    public class EList<T> : List<T> where T : IGUID
    {
        public EList() : base() { }
        public EList(IEnumerable<T> source) : base(source) { }
        public new void Add(T e)
        {
            if (e == null || e.guid == null) return;
            if (Get(e.guid) == null) base.Add(e);
        }
        public void Remove(string guid)
        {
            Remove(Get(guid));
        }
        public T Get(string guid)
        {
            if (guid == null) return default;
            return Find(c => c != null && c.guid != null && c.guid.Equals(guid));
        }
    }
    public static class EListUtils
    {
        public static EList<T> ToEList<T>(this IEnumerable<T> source) where T : IGUID
        {
            if (source == null)
            {
                return null;
            }

            return new EList<T>(source);
        }
    }
}
