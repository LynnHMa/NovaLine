using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NovaElementInspectorWrapper : ScriptableObject
    {
        [SerializeReference] public List<NovaElement> parentElements = new();
        [SerializeReference] public NovaElement selectedElement;
        
        public List<string> ParentElementGuidList => parentElements.ConvertAll(e => e?.Guid);
        public string SelectedElementGuid => selectedElement?.Guid;

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
