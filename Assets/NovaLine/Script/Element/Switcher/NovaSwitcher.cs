using System;
using System.Collections;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
using static NovaLine.Script.Registry.NovaElementRegistry;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class NovaSwitcher : NovaElement,INovaSwitcher
    {
        [SerializeField, HideInInspector] private string _inputElementGUID;
        [SerializeField, HideInInspector] private string _outputElementGUID;

        public override NovaElementType Type => NovaElementType.Switcher;
        public string InputElementGUID  { get => _inputElementGUID;  set => _inputElementGUID  = value; }
        public string OutputElementGUID { get => _outputElementGUID; set => _outputElementGUID = value; }
        
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
            clone._inputElementGUID = null;
            clone._outputElementGUID = null;
            
            return clone;
        }

        public virtual NovaElement TryToFindInputElement()
        {
            return FindElement(InputElementGUID);
        }

        public virtual NovaElement TryToFindOutputElement()
        {
            return FindElement(OutputElementGUID);
        }

        public virtual IEnumerator Next()
        {
            yield break;
        }

        public override void SetParent(NovaElement parent)
        {
            ParentGUID = parent != null ? parent.GUID : "";
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
