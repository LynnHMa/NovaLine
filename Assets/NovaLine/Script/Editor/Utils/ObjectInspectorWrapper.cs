using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectInspectorWrapper : ScriptableObject
    {
        [SerializeReference] public List<NovaElement> parentElements = new();
        [SerializeReference] public NovaElement selectedElement;
        
        public List<string> parentElementGuidList => parentElements.ConvertAll(e => e?.guid);
        public string selectedElementGuid => selectedElement?.guid;

        public new static ObjectInspectorWrapper CreateInstance(string elementGuid)
        {
            var wrapper = CreateInstance<ObjectInspectorWrapper>();
            wrapper.hideFlags = HideFlags.DontSave;

            var selectedElement = NovaElementRegistry.FindElement(elementGuid);
            if (selectedElement != null)
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
