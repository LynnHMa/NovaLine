using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Utils.Interface
{
    public interface IObjectEditor : INovaSwitcher, INovaElement,IEdgeData, IGraphViewNodeData,IGraphViewContext,IGraphNode,IGraphEdge,INovaGraphView
    {
    }
}
