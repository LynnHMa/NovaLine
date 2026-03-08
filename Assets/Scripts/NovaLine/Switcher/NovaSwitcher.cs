using NovaLine.Element;
using NovaLine.Interface;
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
        public NovaElement inputElement { get; set; }
        public NovaElement outputElement { get; set; }
        public string guid { get; set; }
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
    public interface INovaSwitcher
    {

    }
}
