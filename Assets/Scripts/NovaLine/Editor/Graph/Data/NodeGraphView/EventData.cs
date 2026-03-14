using System;

namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    using NovaLine.Editor.Utils.Interface;
    using NovaLine.Event;
    using UnityEngine;

    [Serializable]
    public class EventData : GraphViewNodeData<NovaEvent,IObject,IObject>
    {
        public EventData(NovaEvent novaEvent, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = novaEvent;
        }
    }
}
