using System;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Data.NodeGraphView
{
    using UnityEngine;

    [Serializable]
    public class EventData : GraphViewNodeData<NovaEvent, IObject, IObject>
    {
        public EventData(){}
        public EventData(NovaEvent novaEvent, Vector2 pos)
        {
            Pos = pos;
            LinkedElement = novaEvent;
        }
    }
}
