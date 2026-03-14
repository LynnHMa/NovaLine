using NovaLine.Element;
using NovaLine.Switcher;
using System;
using System.Threading.Tasks;

namespace NovaLine.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
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
            await Task.CompletedTask;
        }

        public override string getType()
        {
            return "[Default Event]";
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is EventSwitcher eventSwitcher && parent is Condition parentCondition)
            {
                var outputEvent = eventSwitcher.outputElement as NovaEvent;
                var inputEvent = eventSwitcher.inputElement as NovaEvent;
                if (outputEvent != null && inputEvent != null)
                {
                    var oIndex = parentCondition.novaEvents.FindIndex(e => e.guid == outputEvent.guid);
                    var iIndex = oIndex + 1;
                    if(iIndex >= 0)
                    {
                        parentCondition?.novaEvents?.Insert(iIndex, inputEvent);
                    }
                }
            }
        }
    }
    public interface INovaEvent
    {

    }
}
