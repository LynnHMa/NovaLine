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

        public NovaElement inputElement  { get => _inputElement;  set => _inputElement  = value; }
        public NovaElement outputElement { get => _outputElement; set => _outputElement = value; }
        public NovaSwitcher()
        {
            guid = Guid.NewGuid().ToString();
        }
        public NovaSwitcher(NovaElement inputElement, NovaElement outputElement, string guid)
        {
            this.inputElement = inputElement;
            this.outputElement = outputElement;
            this.guid = guid;
        }

        public override string getType()
        {
            return "[Edge]";
        }
        public override string getActualName()
        {
            return getType() + " " + "Next Node";
        }
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
