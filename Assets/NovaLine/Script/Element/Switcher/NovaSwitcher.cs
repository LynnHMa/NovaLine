using System;
using System.Collections;
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
            return FindElement(inputElementGuid);
        }

        public virtual NovaElement tryToFindOutputElement()
        {
            return FindElement(outputElementGuid);
        }

        public virtual IEnumerator next()
        {
            yield return null;
        }

        public override void setParent(NovaElement parent)
        {
            parentGuid = parent != null ? parent.guid : "";
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
