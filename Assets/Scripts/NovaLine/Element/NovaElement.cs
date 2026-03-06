using NovaLine.Switcher;
using System;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class NovaElement : INovaElement
    {
        public string name;
        [TextArea]
        public string describtion;
        public virtual string guid { get; set; }
        public virtual INovaElement parent { get; set;}
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
    public interface INovaElement
    {
        public string guid { get; set; }
        public INovaElement parent { get; set; }
    }
}