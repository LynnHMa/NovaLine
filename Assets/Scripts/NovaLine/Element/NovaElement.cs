using NovaLine.Switcher;
using NovaLine.Utils.Interface;
using System;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        //Ignore it,just for unity bug fixing.
        [SerializeField, HideInInspector]
        private bool fuckUnity;

        public string name;
        [TextArea]
        public string describtion;

        [SerializeField, HideInInspector] private string _guid;
        public virtual string guid { get => _guid; set => _guid = value; }

        [HideInInspector]
        public INovaElement parent;
        INovaElement INovaElement.parent { get => parent; set => parent = value; }
        public virtual void onGraphConnect(INovaSwitcher graphEdge) { }
        public virtual void onGraphDisconnect(INovaSwitcher graphEdge) { }

        public static INovaElement getRootElement(INovaElement novaElement,int maxLayer = 1)
        {
            if (maxLayer <= 0) return novaElement;
            return novaElement.parent != null ? getRootElement(novaElement.parent,maxLayer - 1) : novaElement;
        }

        public virtual string getType()
        {
            return "[Default]";
        }
        public virtual string getActualName()
        {
            return getType() + " " + name;
        }
    }
    public interface INovaElement : IGUID
    {
        public INovaElement parent { get; set; }
    }
}