using System;
using NovaLine.Element.Event;

namespace NovaLine.Data.NodeGraphView
{
    using NovaLine.Editor.Utils.Interface;
    using UnityEngine;

    [Serializable]
    public class EventData : GraphViewNodeData<NovaEvent, IObject, IObject>
    {
        public EventData(){}
        public EventData(NovaEvent novaEvent, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = novaEvent;
        }
    }
}
