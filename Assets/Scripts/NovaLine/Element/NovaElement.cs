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
        [SerializeField, HideInInspector] private bool fuckUnity;

        [SerializeReference, HideInInspector] private string _guid;
        [SerializeReference, HideInInspector] private NovaElement _parent;
        [SerializeReference, HideInInspector] private NovaElementType _type;

        public string name;
        [TextArea]
        public string describtion;

        public virtual string guid { get => _guid; set => _guid = value; }
        public virtual NovaElement parent { get => _parent; set => _parent = value;}
        public virtual NovaElementType type { get => _type; set => _type = value; }
        NovaElement INovaElement.parent { get => parent; set => parent = value; }
        public virtual void onGraphConnect(INovaSwitcher graphEdge) { }
        public virtual void onGraphDisconnect(INovaSwitcher graphEdge) { }

        public static INovaElement getRootElement(INovaElement novaElement,int maxLayer = 1)
        {
            if (maxLayer <= 0) return novaElement;
            return novaElement.parent != null ? getRootElement(novaElement.parent,maxLayer - 1) : novaElement;
        }

        public virtual string getTypeName()
        {
            return "[Default]";
        }
        public virtual string getActualName()
        {
            return getTypeName() + " " + name;
        }

        public NovaElement copy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (NovaElement)Activator.CreateInstance(GetType());
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }
    }
    public interface INovaElement : IGUID
    {
        public NovaElement parent { get; set; }
        public NovaElementType type { get; set; }
        public NovaElement copy();
    }
    public enum NovaElementType
    {
        NONE,
        FLOWCHART,
        NODE,
        ACTION,
        CONDITION,
        EVENT,
        SWITCHER
    }
}