using System;
using System.Threading.Tasks;

namespace NovaLine.Element.Event
{
    [Serializable]
    public class WaitingEvent : NovaEvent
    {
        public int ms;
        public WaitingEvent() : base()
        {
            ms = 1000;
        }
        public WaitingEvent(int ms) : base()
        {
            this.ms = ms;
        }

        public override async Task onEvent()
        {
            await Task.Delay(ms);
            await base.onEvent();
        }

        public override string getTypeName()
        {
            return "[Waiting Event]";
        }
    }
}
