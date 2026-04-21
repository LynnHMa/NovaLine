using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window.Context.Edge
{
    public class EdgeContext : NovaContext<IEdgeData>
    {
        public EdgeContext(IEdgeData linkedData) : base(linkedData){}

        public override void SaveData()
        {
            if (LinkedData is NodeEdgeData nodeEdgeData)
            {
                if (GetContext(nodeEdgeData.SwitchConditionData.Guid,NovaElementType.Condition) is ConditionContext switchConditionContext)
                {
                    switchConditionContext.SaveData();
                    nodeEdgeData.SwitchConditionData = switchConditionContext.LinkedData;
                }
            }
        }
    }
}