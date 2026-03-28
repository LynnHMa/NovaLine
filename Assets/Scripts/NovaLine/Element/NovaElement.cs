using NovaLine.Utils.Interface;
using System;
using System.Collections.Generic;
using NovaLine.Element.Switcher;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        //Ignore it,just for unity bug fixing.
        [SerializeField, HideInInspector] private bool fuckUnity;

        [SerializeReference,HideInInspector] private string _guid;
        [SerializeReference,HideInInspector] private NovaElement _parent;
        [SerializeReference,HideInInspector] private List<NovaElement> _children = new();
        [SerializeReference,HideInInspector] private NovaElement _firstChild;
        [SerializeReference,HideInInspector] private List<NovaSwitcher> _switchers = new();
        [SerializeReference,HideInInspector] private NovaElementType _type;

        public string name;
        [TextArea]
        public string describtion;

        public virtual string guid { get => _guid; set => _guid = value; }

        public virtual NovaElement parent { get => _parent; set => _parent = value; }
        public virtual List<NovaElement> children { get => _children; set => _children = value; }
        public virtual NovaElement firstChild { get => _firstChild; set => _firstChild = value; }
        public virtual List<NovaSwitcher> switchers { get => _switchers; set => _switchers = value; }
        public virtual NovaElementType type { get => _type; set => _type = value; }
        NovaElement INovaElement.parent { get => parent; set => parent = value; }
        public virtual void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is NovaSwitcher switcher)
            {
                if (switchers == null) switchers = new();
                if (!switchers.Exists(s => s.guid == switcher.guid))
                {
                    switchers.Add(switcher);
                }
            }
        }
        public virtual void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is NovaSwitcher switcher)
            {
                if (switchers == null) switchers = new();
                switchers.Remove(switcher);
            }
        }

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

        public virtual NovaElement strongCopy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (NovaElement)Activator.CreateInstance(GetType());
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public virtual NovaElement copy()
        {
            var clone = strongCopy();
            clone.guid = Guid.NewGuid().ToString();
            
            //Waiting to be written by CopyPasteHelper.
            children.Clear();
            switchers.Clear();
            firstChild = null;
            
            return clone;
        }

        public virtual void addChild(NovaElement child)
        {
            child.parent = this;
            children.Add(child);
        }

        public virtual void removeChild(NovaElement child)
        {
            child.parent = null;
            children.Remove(child);
        }
    }
    public interface INovaElement : IGUID
    {
        public NovaElement parent { get; set; }
        public NovaElementType type { get; set; }
        public NovaElement strongCopy();
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