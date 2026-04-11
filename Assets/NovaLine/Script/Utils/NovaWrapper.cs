using System;
using UnityEngine;

namespace NovaLine.Script.Utils
{
    [Serializable]
    public class NovaWrapper<T>
    {
        [SerializeReference] private T _value;

        public T value
        {
            get => _value;
            set => _value = value;
        }
        public NovaWrapper(){}
        public NovaWrapper(T value)
        {
            this.value = value;
        }
    }
}