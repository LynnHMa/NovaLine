using System;
using UnityEngine;

namespace NovaLine.Script.Utils
{
    [Serializable]
    public class NovaWrapper<T>
    {
        [SerializeReference] private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
        public NovaWrapper(){}
        public NovaWrapper(T value)
        {
            Value = value;
        }
    }
}