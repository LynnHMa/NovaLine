using System;
using System.Collections;
using NovaLine.Script.UI.Container;
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

        public override NovaElementType Type => NovaElementType.Switcher;
        public string InputElementGuid  { get => _inputElementGuid;  set => _inputElementGuid  = value; }
        public string OutputElementGuid { get => _outputElementGuid; set => _outputElementGuid = value; }
        
        public NovaSwitcher()
        {
            name = "Next";
        }

        public override string GetTypeName()
        {
            return "[Edge]";
        }
        public override NovaElement Copy()
        {
            if (base.Copy() is not NovaSwitcher clone) return null;
            
            //Waiting to be written by CopyPasteHelper.
            clone._inputElementGuid = null;
            clone._outputElementGuid = null;
            
            return clone;
        }

        public virtual NovaElement TryToFindInputElement()
        {
            return FindElement(InputElementGuid);
        }

        public virtual NovaElement TryToFindOutputElement()
        {
            return FindElement(OutputElementGuid);
        }

        public virtual IEnumerator Next()
        {
            yield break;
        }

        public override void SetParent(NovaElement parent)
        {
            ParentGuid = parent != null ? parent.Guid : "";
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
