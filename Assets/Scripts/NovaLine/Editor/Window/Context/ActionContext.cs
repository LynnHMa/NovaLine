using NovaLine.Editor.Utils.Interface;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.NodeGraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class ActionContext : GraphViewContext<IObjectEditor, ActionData>
    {
        public ActionContext(ActionData linkedData) : base(linkedData) { }
        public override void save()
        {
        }

        protected override void cleanInvalidChild()
        {
        }
        //No action graph view
        protected override IObjectEditor summonGraphView()
        {
            return null;
        }
    }
}
