using NovaLine.Element;
using NovaLine.Interface;
using NovaLine.Switcher;
using System;
using System.Threading.Tasks;

namespace NovaLine.Event
{
    [Serializable]
    public abstract class NovaEvent : NovaElement
    {
        public new string name => getType();

        public virtual async Task onEvent()
        {
            await Task.CompletedTask;
        }

        public virtual string getType()
        {
            return "Empty Event";
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
}
