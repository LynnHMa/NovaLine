using NovaLine.Element;

namespace NovaLine.Editor.Utils
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ObjectInspectorWrapper : ScriptableObject
    {
        [SerializeReference]
        public List<object> parentNodes = new();

        [SerializeReference]
        public object selectedNodeInfo;

        [SerializeReference]
        public Toggle setToStart;

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
