using System;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class NovaSwitcher : NovaElement,INovaSwitcher
    {
        [SerializeField, HideInInspector] private string _inputElementGuid;
        [SerializeField, HideInInspector] private string _outputElementGuid;

        public override NovaElementType type => NovaElementType.SWITCHER;
        public string inputElementGuid  { get => _inputElementGuid;  set => _inputElementGuid  = value; }
        public string outputElementGuid { get => _outputElementGuid; set => _outputElementGuid = value; }
        public NovaSwitcher()
        {
            name = "Next Node";
        }

        public override string getTypeName()
        {
            return "[Edge]";
        }
        public override NovaElement copy()
        {
            var clone = base.copy() as NovaSwitcher;
            if (clone == null) return null;
            
            //Waiting to be written by CopyPasteHelper.
            clone._inputElementGuid = null;
            clone._outputElementGuid = null;
            
            return clone;
        }

        public virtual NovaElement tryToFindInputElement()
        {
            if (parent == null) return null;
            foreach (var childGuid in parent.childrenGuidList)
            {
                var child = FindElement(childGuid);
                if (child.guid.Equals(_inputElementGuid)) return child;
            }

            return null;
        }

        public virtual NovaElement tryToFindOutputElement()
        {
            if (parent == null) return null;
            foreach (var childGuid in parent.childrenGuidList)
            {
                var child = FindElement(childGuid);
                if (child.guid.Equals(_outputElementGuid)) return child;
            }

            return null;
        }

        public override void setParent(NovaElement parent)
        {
            parentGuid = parent.guid;
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
