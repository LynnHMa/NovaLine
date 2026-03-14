using NovaLine.Element;
using NovaLine.Utils.Interface;
using System;
using UnityEngine;

namespace NovaLine.Switcher
{
    [Serializable]
    public class NovaSwitcher : INovaSwitcher
    {
        //Ignore it,just for unity bug fixing.
        [SerializeField, HideInInspector]
        private bool fuckUnity;

        [SerializeReference] private NovaElement _inputElement;
        [SerializeReference] private NovaElement _outputElement;
        [SerializeField]     private string _guid;

        public NovaElement inputElement  { get => _inputElement;  set => _inputElement  = value; }
        public NovaElement outputElement { get => _outputElement; set => _outputElement = value; }
        public string guid               { get => _guid;          set => _guid          = value; }
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
    }
    public interface INovaSwitcher : IGUID
    {
    }
}
