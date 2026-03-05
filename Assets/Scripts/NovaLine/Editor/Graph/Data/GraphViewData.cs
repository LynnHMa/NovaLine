using System;

namespace NovaLine.Editor.Graph.Data
{
    using UnityEngine;

    [Serializable]
    public class GraphViewData : ScriptableObject, IGraphViewData
    {
        public virtual new string name { get; set; }

        public virtual string describtion { get; set; }

        public virtual Vector2 pos { get; set; }

        public virtual string guid { get; set; }
    }
    public interface IGraphViewData
    {
        public string name { get; set; }

        public string describtion { get; set; }

        public Vector2 pos { get; set; }

        public string guid { get; set; }
    }
}
