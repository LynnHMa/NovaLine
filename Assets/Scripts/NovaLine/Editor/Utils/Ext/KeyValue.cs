using System;

namespace Editor.Utils.Ext
{
    [Serializable]
    public class KeyValue<K, V>
    {
        public K key;
        public V value;
        public KeyValue(K key, V value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
