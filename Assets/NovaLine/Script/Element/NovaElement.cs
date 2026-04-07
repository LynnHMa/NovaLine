using NovaLine.Script.Utils.Interface;
using System;
using System.Collections.Generic;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        //Ignore it,just for unity bug fixing.
        [SerializeField, HideInInspector] private bool fuckUnity;
        
        [SerializeField,HideInInspector] private string _guid;
        [SerializeField,HideInInspector] private NovaElementType _type;
        [SerializeField,HideInInspector] private string _parentGuid;
        [SerializeField,HideInInspector] private string _firstChildGuid;

        [NonSerialized] private List<string> _childrenGuidList = new();
        [NonSerialized] private List<string> _switchersGuidList = new();
        
        public string name;
        [TextArea] public string describtion;

        public virtual NovaElement parent => FindElement(_parentGuid);
        public virtual NovaElement firstChild => FindElement(_firstChildGuid);
        public virtual string parentGuid
        {
            get => _parentGuid;
            set => _parentGuid = value;
        }
        public string firstChildGuid
        {
            get => _firstChildGuid;
            set => _firstChildGuid = value;
        }
        public virtual List<string> childrenGuidList 
        { 
            get => _childrenGuidList; 
            set => _childrenGuidList = value; 
        }
        public virtual string guid
        {
            get => _guid; 
            set => _guid = value;
        }
        public virtual List<string> switchersGuidList 
        { 
            get => _switchersGuidList; 
            set => _switchersGuidList = value; 
        }
        public virtual NovaElementType type 
        {
            get => _type; 
            set => _type = value; 
        }

        string INovaElement.parentGuid
        {
            get => parentGuid; 
            set => parentGuid = value;
        }
        public NovaElement() {
            guid = Guid.NewGuid().ToString();
            RegisterElement(this);
        }
        public virtual void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (switchersGuidList == null) switchersGuidList = new();
            if (!switchersGuidList.Contains(graphEdge.guid))
            {
                switchersGuidList.Add(graphEdge.guid);
            }
        }
        public virtual void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            if (switchersGuidList == null) switchersGuidList = new();
            switchersGuidList.Remove(graphEdge.guid);
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
            UnregisterElement(clone.guid);
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public virtual NovaElement copy()
        {
            var clone = strongCopy();
            clone.guid = Guid.NewGuid().ToString();
            RegisterElement(clone);
            return clone;
        }

        public virtual void map(NovaElement oldElement)
        {
            string beforeJson = JsonUtility.ToJson(oldElement);
            JsonUtility.FromJsonOverwrite(beforeJson, this);
        }

        public virtual void setParent(NovaElement parent)
        {
            if (this.parent != null)
            {
                this.parent.childrenGuidList.Remove(guid);
            }
            if (parent != null)
            {
                parentGuid = parent.guid;
                parent.childrenGuidList.Add(guid);
            }
        }
    }
    public interface INovaElement : IGUID
    {
        public NovaElement parent { get;}
        public string parentGuid { get; set; }
        public NovaElement firstChild { get;}
        public string firstChildGuid { get; set; }
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