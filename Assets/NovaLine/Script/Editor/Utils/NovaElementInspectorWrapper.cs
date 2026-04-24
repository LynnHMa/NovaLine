using NovaLine.Script.Element;
using System.Collections.Generic;
using NovaLine.Script.Registry;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils
{
    public class NovaElementInspectorWrapper : ScriptableObject
    {
        [SerializeReference] public List<NovaElement> parentElements = new();
        [SerializeReference] public NovaElement selectedElement;
        
        public List<string> ParentElementGUIDList => parentElements.ConvertAll(e => e?.GUID);
        public string SelectedElementGUID => selectedElement?.GUID;

        public NovaElement FindParent(string guid)
        {
            return parentElements.Find(e => e.GUID == guid);
        }
        public new static NovaElementInspectorWrapper CreateInstance(string elementGUID)
        {
            var wrapper = CreateInstance<NovaElementInspectorWrapper>();
            wrapper.hideFlags = HideFlags.DontSave;

            var selectedElement = NovaElementRegistry.FindElement(elementGUID);
            if (selectedElement != null)
            {
                wrapper.selectedElement = selectedElement;

                var p = selectedElement.Parent;
                while (p != null)
                {
                    wrapper.parentElements.Add(p);
                    p = p.Parent;
                }
                wrapper.parentElements.Reverse();
                wrapper.name = selectedElement.GetActualName();
            }

            return wrapper;
        }
    }
}
