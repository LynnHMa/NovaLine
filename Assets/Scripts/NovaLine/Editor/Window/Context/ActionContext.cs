using NovaLine.Editor.Utils.Interface;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class ActionContext : GraphViewContext<IObject, ActionData>
    {
        public override ContextType type => ContextType.ACTION;
        public ActionContext(ActionData linkedData) : base(linkedData) { }
        public override void save()
        {
        }

        public void saveConditionData()
        {

        }
        //No action graph view
        protected override IObject summonGraphView()
        {
            return null;
        }
    }
}
