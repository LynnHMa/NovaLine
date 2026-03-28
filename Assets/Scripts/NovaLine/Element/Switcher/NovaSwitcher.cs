using System;
using NovaLine.Utils.Interface;
using UnityEngine;

namespace NovaLine.Element.Switcher
{
    [Serializable]
    public class NovaSwitcher : NovaElement,INovaSwitcher
    {
        [SerializeReference,HideInInspector] private NovaElement _inputElement;
        [SerializeReference, HideInInspector] private NovaElement _outputElement;

        public override NovaElementType type => NovaElementType.SWITCHER;
        public NovaElement inputElement  { get => _inputElement;  set => _inputElement  = value; }
        public NovaElement outputElement { get => _outputElement; set => _outputElement = value; }
        public NovaSwitcher()
        {
            guid = Guid.NewGuid().ToString();
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
            clone._inputElement = null;
            clone._outputElement = null;
            
            return clone;
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
