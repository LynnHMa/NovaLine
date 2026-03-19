using NovaLine.Element;
using NovaLine.Switcher;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
        [HideInInspector]
        public EventSwitcher nextEvent;
        public NovaEvent()
        {
            guid = Guid.NewGuid().ToString();
        }

        public NovaEvent(string name) : this()
        {
            this.name = name;
        }

        public NovaEvent(string name, string guid) : this(name)
        {
            this.guid = guid;
        }
        public virtual async Task onEvent()
        {
            if (parent != null && parent is Condition parentCondition && parentCondition.type == ConditionType.Sort)
            {
                var nextEvent = (NovaEvent)this.nextEvent.inputElement;
                await nextEvent?.onEvent();
            }
            else await Task.CompletedTask;
        }

        public override string getType()
        {
            return "[Default Event]";
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is EventSwitcher eventSwitcher)
            {
                nextEvent = eventSwitcher;
            }
        }
        public override void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            nextEvent = null;
        }
    }
    public interface INovaEvent
    {

    }
}
