using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window.Context.Edge
{
    public class EdgeContext : NovaContext<IEdgeData>
    {
        public EdgeContext(IEdgeData linkedData) : base(linkedData){}

        public override void saveData()
        {
            if (linkedData is NodeEdgeData nodeEdgeData)
            {
                if (GetContext(nodeEdgeData.switchConditionData.guid,NovaElementType.CONDITION) is ConditionContext switchConditionContext)
                {
                    switchConditionContext.saveData();
                }
            }
        }
    }
}