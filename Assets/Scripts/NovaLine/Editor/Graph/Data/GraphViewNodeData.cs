using NovaLine.Editor.Graph.View;
using System;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public class GraphViewNodeData<T> : GraphViewData,IGraphViewNodeData
    {
        public virtual void draw(INovaGraphView graphView) { }
        public virtual T to() { return default; }
        public virtual void update(T element) { }
    }
    public interface IGraphViewNodeData : IGraphViewData
    {
        public virtual void draw(INovaGraphView graphView) { }
    }
}
