﻿using NovaLine.Script.Utils.Interface;
using System;
using System.Collections.Generic;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using static NovaLine.Script.Registry.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        //Ignore it,just for unity bug fixing.
        [SerializeField, HideInInspector] private bool fuckUnity;
        
        [SerializeField,HideInInspector] private string _GUID;
        [SerializeField,HideInInspector] private NovaElementType _type;
        [SerializeField,HideInInspector] private string _parentGUID;
        [SerializeField,HideInInspector] private string _firstChildGUID;

        [NonSerialized] private List<string> _childrenGUIDList = new();
        [NonSerialized] private List<string> _switchersGUIDList = new();
        
        public string name;
        [TextArea] public string description;

        public virtual Color ThemedColor => Color.white;
        public virtual NovaElement Parent => FindElement(_parentGUID);
        public virtual NovaElement FirstChild => FindElement(_firstChildGUID);
        public virtual string ParentGUID
        {
            get => _parentGUID;
            set => _parentGUID = value;
        }
        public string FirstChildGUID
        {
            get => _firstChildGUID;
            set => _firstChildGUID = value;
        }
        public virtual List<string> ChildrenGUIDList 
        { 
            get => _childrenGUIDList; 
            set => _childrenGUIDList = value; 
        }
        public virtual string GUID
        {
            get => _GUID; 
            set => _GUID = value;
        }
        public virtual List<string> SwitchersGUIDList 
        { 
            get => _switchersGUIDList; 
            set => _switchersGUIDList = value; 
        }
        public virtual NovaElementType Type 
        {
            get => _type; 
            set => _type = value; 
        }

        string INovaElement.ParentGUID
        {
            get => ParentGUID; 
            set => ParentGUID = value;
        }
        public NovaElement() {
            GUID = System.Guid.NewGuid().ToString();
            RegisterElement(this);
        }
        public virtual void OnGraphConnect(INovaSwitcher graphEdge)
        {
            SwitchersGUIDList ??= new();
            if (!SwitchersGUIDList.Contains(graphEdge.GUID))
            {
                SwitchersGUIDList.Add(graphEdge.GUID);
            }
        }
        public virtual void OnGraphDisconnect(INovaSwitcher graphEdge)
        {
            SwitchersGUIDList ??= new();
            SwitchersGUIDList.Remove(graphEdge.GUID);
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
            UnregisterElement(clone.GUID);
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public virtual NovaElement Copy()
        {
            var clone = StrongCopy();
            clone.GUID = System.Guid.NewGuid().ToString();
            clone.ChildrenGUIDList?.Clear();
            clone.SwitchersGUIDList?.Clear();
            clone.ParentGUID = "";
            clone.FirstChildGUID = "";
            RegisterElement(clone);
            return clone;
        }

        public virtual void SetParent(NovaElement parent)
        {
            if (Parent != null)
            {
                Parent.ChildrenGUIDList?.Remove(GUID);
            }

            ParentGUID = parent != null ? parent.GUID : "";

            if (parent == null) return;

            parent.ChildrenGUIDList ??= new List<string>();
            if (!parent.ChildrenGUIDList.Contains(GUID))
            {
                parent.ChildrenGUIDList.Add(GUID);
            }
        }
    }
    public interface INovaElement : IGUID
    {
        public NovaElement Parent { get;}
        public string ParentGUID { get; set; }
        public NovaElement FirstChild { get;}
        public string FirstChildGUID { get; set; }
        public NovaElementType Type { get; set; }
        public NovaElement StrongCopy();
        public NovaElement Copy();
    }
    public enum NovaElementType
    {
        None,
        Flowchart,
        Node,
        Action,
        Condition,
        Event,
        Switcher
    }
}