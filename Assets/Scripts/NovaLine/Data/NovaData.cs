using System;

namespace NovaLine.Data
{
    using NovaLine.Utils.Interface;
    using UnityEngine;

    [Serializable]
    public abstract class NovaData : INovaData
    {
        [SerializeField] private string _name;
        [SerializeField] private string _describtion;
        [SerializeField] private Vector2 _pos;
        [SerializeField] private string _guid;

        public virtual string name { get => _name; set => _name = value; }
        public virtual string describtion { get => _describtion; set => _describtion = value; }
        public virtual Vector2 pos { get => _pos; set => _pos = value; }
        public virtual string guid { get => _guid; set => _guid = value; }
        public Vector2 getPos() => _pos;
        
        protected NovaData(){}

        public virtual INovaData copy()
        {
            return strongCopy();
        }

        public virtual NovaData strongCopy()
        {
            var data = JsonUtility.ToJson(this);
            var clone = (NovaData)Activator.CreateInstance(GetType());
            JsonUtility.FromJsonOverwrite(data, clone);
            return clone;
        }
    }
    public interface INovaData : IGUID
    {
        public string name { get; set; }

        public string describtion { get; set; }

        public Vector2 pos { get; set; }
        INovaData copy();
        NovaData strongCopy();
    }
}
