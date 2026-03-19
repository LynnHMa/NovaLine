using NovaLine.Element;

namespace NovaLine.Editor.Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectInspectorWrapper : ScriptableObject
    {
        [SerializeReference]
        public List<object> parentElements = new();

        [SerializeReference]
        public object selectedElement;

        public static ObjectInspectorWrapper CreateInstance(object selectObj)
        {
            var wrapper = CreateInstance<ObjectInspectorWrapper>();

            wrapper.hideFlags = HideFlags.DontSave;

            if (selectObj is NovaElement selectedElement)
            {
                wrapper.selectedElement = selectedElement;

                var p = selectedElement.parent;
                while (p != null)
                {
                    wrapper.parentElements.Add(p);
                    p = p.parent;
                }
                wrapper.parentElements.Reverse();

                wrapper.name = selectedElement.getActualName();
            }

            return wrapper;
        }
    }
}
