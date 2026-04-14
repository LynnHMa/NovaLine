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

        public virtual TNovaElement LinkedElement 
        { 
            get => _linkedElement; 
            set => _linkedElement = value; 
        }
        public virtual string Name => LinkedElement?.name;
        public virtual string Description => LinkedElement?.description;
        public virtual string Guid => LinkedElement?.Guid;
        public virtual Vector2 Pos { get => _pos; set => _pos = value; }
        public Vector2 GetPos() => _pos;

        NovaElement INovaData.LinkedElement
        {
            get => LinkedElement;
            set => LinkedElement = (TNovaElement)value;
        }
        
        protected NovaData(){}

        public virtual INovaData Copy()
        {
            return StrongCopy();
        }

        public virtual INovaData StrongCopy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (INovaData)Activator.CreateInstance(GetType());
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }

        public abstract void RegisterLinkedElement();
        public abstract void UpdateLinkedElement(bool updateChildren = true);
    }
    public interface INovaData : IGUID
    {
        NovaElement LinkedElement { get; set; }
        public string Name { get;}

        public string Description { get;}
        public Vector2 Pos { get; set; }
        INovaData Copy();
        INovaData StrongCopy();
        void RegisterLinkedElement();
        void UpdateLinkedElement(bool updateChildren = true);
    }
}
