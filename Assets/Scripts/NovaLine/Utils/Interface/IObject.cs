using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using NovaLine.Element;
using NovaLine.Element.Switcher;

namespace NovaLine.Editor.Utils.Interface
{
    public interface IObject : INovaSwitcher, INovaElement,IEdgeData, IGraphViewNodeData
    {
    }
}
