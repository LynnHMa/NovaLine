using System;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
namespace NovaLine.Script.Data
{
    [Serializable]
    public abstract class NovaData<TNovaElement> : INovaData where TNovaElement : NovaElement
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private Vector2 _pos;
        [SerializeField] private string _guid;
        [SerializeReference] private TNovaElement _linkedElement;

        public virtual TNovaElement linkedElement 
        { 
            get => _linkedElement; 
            set => _linkedElement = value; 
        }
        public virtual string name => linkedElement?.name;
        public virtual string description => linkedElement?.description;
        public virtual string Guid => linkedElement?.Guid;
        public virtual Vector2 pos { get => _pos; set => _pos = value; }
        public Vector2 getPos() => _pos;

        NovaElement INovaData.linkedElement
        {
            get => linkedElement;
            set => linkedElement = (TNovaElement)value;
        }
        
        protected NovaData(){}

        public virtual INovaData copy()
        {
            return strongCopy();
        }

        public virtual INovaData strongCopy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (INovaData)Activator.CreateInstance(GetType());
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public abstract void registerLinkedElement();
        public abstract void updateLinkedElement(bool updateChildren = true);
    }
    public interface INovaData : IGUID
    {
        NovaElement linkedElement { get; set; }
        public string name { get;}

        public string description { get;}
        public Vector2 pos { get; set; }
        INovaData copy();
        INovaData strongCopy();
        void registerLinkedElement();
        void updateLinkedElement(bool updateChildren = true);
    }
}
