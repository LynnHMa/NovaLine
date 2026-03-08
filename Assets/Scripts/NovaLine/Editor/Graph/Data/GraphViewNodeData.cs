using NovaLine.Editor.Graph.View;
using NovaLine.Element;
using System;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public abstract class GraphViewNodeData<T> : GraphViewData,IGraphViewNodeData where T : NovaElement
    {
        public override string name => linkedElement?.name;

        public override string describtion => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;

        public virtual string startGraphNodeGuid { get; set; }
        public virtual T linkedElement { get; set; }
        public virtual void draw(INovaGraphView graphView) { }
    }
    public interface IGraphViewNodeData : IGraphViewData
    {
        public virtual void draw(INovaGraphView graphView) { }
    }
}
