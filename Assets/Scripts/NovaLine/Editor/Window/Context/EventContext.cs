using NovaLine.Editor.Utils.Interface;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.NodeGraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class EventContext : GraphViewContext<IObjectEditor, EventData>
    {
        public EventContext(EventData linkedData) : base(linkedData) { }
        public override void save()
        {
        }
        protected override void cleanInvalidChild()
        {
        }
        //No event graph view
        protected override IObjectEditor summonGraphView()
        {
            return null;
        }
    }
}
