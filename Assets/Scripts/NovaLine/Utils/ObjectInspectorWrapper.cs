using UnityEngine;

namespace NovaLine.Utils
{
    public class ObjectInspectorWrapper : ScriptableObject
    {
        [SerializeReference]
        public object objectData;
    }
}
