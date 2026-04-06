using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Utils.Interface
{
    public interface IObject : INovaSwitcher, INovaElement,IEdgeData, IGraphViewNodeData
    {
    }
}
