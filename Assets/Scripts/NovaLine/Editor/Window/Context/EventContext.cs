using NovaLine.Editor.Utils.Interface;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class EventContext : GraphViewContext<IObject, EventData>
    {
        public EventContext(EventData linkedData) : base(linkedData) { }
        public override void save()
        {
        }

        //No event graph view
        protected override IObject summonGraphView()
        {
            return null;
        }
    }
}
