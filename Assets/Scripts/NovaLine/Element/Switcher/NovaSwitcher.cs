using NovaLine.Element;
using NovaLine.Utils.Interface;
using System;
using UnityEngine;

namespace NovaLine.Switcher
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
        public override string getActualName()
        {
            return getTypeName() + " " + name;
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
