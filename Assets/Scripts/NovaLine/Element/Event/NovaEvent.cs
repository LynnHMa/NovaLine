using System;
using System.Linq;
using System.Threading.Tasks;
using NovaLine.Element.Switcher;
using UnityEngine;

namespace NovaLine.Element.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
        public override NovaElementType type => NovaElementType.EVENT;
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
                var nextEvent = (NovaEvent)switchers.FirstOrDefault()?.inputElement;
                if (nextEvent != null)
                {
                    await nextEvent?.onEvent();
                }
            }
            else await Task.CompletedTask;
        }

        public override string getTypeName()
        {
            return "[Default Event]";
        }
    }
    public interface INovaEvent
    {
    }
}
