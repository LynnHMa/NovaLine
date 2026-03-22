using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Editor.Window.Context;
using NovaLine.Element;
using NovaLine.Switcher;

namespace NovaLine.Editor.Utils.Interface
{
    public interface IObjectEditor : INovaSwitcher, INovaElement,IEdgeData, IGraphViewNodeData,IGraphViewContext,IGraphNode,IGraphEdge,INovaGraphView
    {
    }
}
