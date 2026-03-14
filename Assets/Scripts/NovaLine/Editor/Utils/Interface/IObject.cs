using NovaLine.Editor.Graph.Data.Edge;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Editor.Window.Context;
using NovaLine.Element;
using NovaLine.Switcher;

namespace NovaLine.Editor.Utils.Interface
{
    public interface IObject : INovaSwitcher, INovaElement,IEdgeData, IGraphViewNodeData,IGraphViewContext,IGraphNode,IGraphEdge,INovaGraphView
    {
    }
}
