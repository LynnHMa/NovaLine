using NovaLine.Event;
using NovaLine.Switcher;
using System;
using UnityEngine;

namespace NovaLine.Editor.Graph.Data.Edge
{
    [Serializable]
    public class EventEdgeData : EdgeData<NovaEvent, EventSwitcher>
    {
        [SerializeReference] private EventSwitcher _linkedSwitcher;

        public override EventSwitcher linkedSwitcher
        {
            get => _linkedSwitcher;
            set => _linkedSwitcher = value;
        }

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
