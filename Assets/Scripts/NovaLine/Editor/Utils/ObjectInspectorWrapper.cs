using NovaLine.Element;

namespace NovaLine.Editor.Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectInspectorWrapper : ScriptableObject
    {
        [SerializeReference]
        public List<object> parentNodes = new();

        [SerializeReference]
        public object selectedNodeInfo;

        public static ObjectInspectorWrapper CreateInstance(object selectedNodeInfo)
        {
            var result = CreateInstance<ObjectInspectorWrapper>();

            result.hideFlags = HideFlags.DontSave;
            result.name = "Selected Node";

            result.selectedNodeInfo = selectedNodeInfo;

            if (selectedNodeInfo is NovaElement currentElement)
            {
                var p = currentElement.parent;
                while (p != null)
                {
                    result.parentNodes.Add(p);
                    p = p.parent;
                }
                result.parentNodes.Reverse();
            }

            return result;
        }
    }
}
