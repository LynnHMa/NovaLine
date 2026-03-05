
using NovaLine.Interface;
using NovaLine.Switcher;
using System;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        [TextArea]
        public string describtion;
        public string name;
        public virtual string guid { get; set; }
        public virtual INovaElement parent { get; set;}
        public virtual void onGraphConnect(INovaSwitcher graphEdge) { }
        public virtual void onGraphDisconnect(INovaSwitcher graphEdge) { }
    }
    public interface INovaElement
    {
        public string guid { get; set; }
        public INovaElement parent { get; set; }
    }
}