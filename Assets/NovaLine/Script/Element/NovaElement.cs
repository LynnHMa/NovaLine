﻿using NovaLine.Script.Utils.Interface;
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
        [TextArea] public string description;

        public virtual NovaElement Parent => FindElement(_parentGuid);
        public virtual NovaElement FirstChild => FindElement(_firstChildGuid);
        public virtual string ParentGuid
        {
            get => _parentGuid;
            set => _parentGuid = value;
        }
        public string FirstChildGuid
        {
            get => _firstChildGuid;
            set => _firstChildGuid = value;
        }
        public virtual List<string> ChildrenGuidList 
        { 
            get => _childrenGuidList; 
            set => _childrenGuidList = value; 
        }
        public virtual string Guid
        {
            get => _guid; 
            set => _guid = value;
        }
        public virtual List<string> SwitchersGuidList 
        { 
            get => _switchersGuidList; 
            set => _switchersGuidList = value; 
        }
        public virtual NovaElementType Type 
        {
            get => _type; 
            set => _type = value; 
        }

        string INovaElement.ParentGuid
        {
            get => ParentGuid; 
            set => ParentGuid = value;
        }
        public NovaElement() {
            Guid = System.Guid.NewGuid().ToString();
            RegisterElement(this);
        }
        public virtual void OnGraphConnect(INovaSwitcher graphEdge)
        {
            SwitchersGuidList ??= new();
            if (!SwitchersGuidList.Contains(graphEdge.Guid))
            {
                SwitchersGuidList.Add(graphEdge.Guid);
            }
        }
        public virtual void OnGraphDisconnect(INovaSwitcher graphEdge)
        {
            SwitchersGuidList ??= new();
            SwitchersGuidList.Remove(graphEdge.Guid);
        }

        public static INovaElement GetRootElement(INovaElement novaElement,int maxLayer = 1)
        {
            if (maxLayer <= 0) return novaElement;
            return novaElement.Parent != null ? GetRootElement(novaElement.Parent,maxLayer - 1) : novaElement;
        }

        public virtual string GetTypeName()
        {
            return "[Default]";
        }
        public virtual string GetActualName()
        {
            return GetTypeName() + " " + name;
        }

        public virtual NovaElement StrongCopy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (NovaElement)Activator.CreateInstance(GetType());
            UnregisterElement(clone.Guid);
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public virtual NovaElement Copy()
        {
            var clone = StrongCopy();
            clone.Guid = System.Guid.NewGuid().ToString();
            RegisterElement(clone);
            return clone;
        }

        public virtual void SetParent(NovaElement parent)
        {
            if (this.Parent != null)
            {
                this.Parent.ChildrenGuidList?.Remove(Guid);
            }

            ParentGuid = parent != null ? parent.Guid : "";

            if (parent == null) return;

            parent.ChildrenGuidList ??= new List<string>();
            if (!parent.ChildrenGuidList.Contains(Guid))
            {
                parent.ChildrenGuidList.Add(Guid);
            }
        }
    }
    public interface INovaElement : IGUID
    {
        public NovaElement Parent { get;}
        public string ParentGuid { get; set; }
        public NovaElement FirstChild { get;}
        public string FirstChildGuid { get; set; }
        public NovaElementType Type { get; set; }
        public NovaElement StrongCopy();
        public NovaElement Copy();
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