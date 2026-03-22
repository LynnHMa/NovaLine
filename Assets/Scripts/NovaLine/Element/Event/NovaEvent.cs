using System;
using System.Threading.Tasks;
using NovaLine.Switcher;
using UnityEngine;

namespace NovaLine.Element.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
        public override NovaElementType type => NovaElementType.EVENT;
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
        public virtual async Task onEvent()
        {
            if (parent != null && parent is Condition parentCondition && parentCondition.conditionType == ConditionType.Sort)
            {
                var nextEvent = (NovaEvent)this.nextEvent.inputElement;
                await nextEvent?.onEvent();
            }
            else await Task.CompletedTask;
        }

        public override string getTypeName()
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
