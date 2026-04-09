using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class EventEdgeData : EdgeData<EventSwitcher>
    {
        public override string name => "Next Event";
        public override string description => "The next event.";

        public EventEdgeData()
        {
        }
        public EventEdgeData(EventSwitcher element) : base(element)
        {
        }
    }
}
