using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class EventEdgeData : EdgeData<EventSwitcher>
    {
        public override string name => "Next Event";
        public override string describtion => "The next event.";

        public EventEdgeData()
        {
        }
        public EventEdgeData(EventSwitcher switcher) : base(switcher)
        {
        }
    }
}
