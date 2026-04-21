using NovaLine.Script.Element;
using System.Collections.Generic;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils
{
    public class NovaElementInspectorWrapper : ScriptableObject
    {
        [SerializeReference] public List<NovaElement> parentElements = new();
        [SerializeReference] public NovaElement selectedElement;
        
        public List<string> ParentElementGuidList => parentElements.ConvertAll(e => e?.Guid);
        public string SelectedElementGuid => selectedElement?.Guid;

        public NovaElement FindParent(string guid)
        {
            return parentElements.Find(e => e.Guid == guid);
        }
        public new static NovaElementInspectorWrapper CreateInstance(string elementGuid)
        {
            var wrapper = CreateInstance<NovaElementInspectorWrapper>();
            wrapper.hideFlags = HideFlags.DontSave;

            var selectedElement = NovaElementRegistry.FindElement(elementGuid);
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
