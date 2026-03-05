using System;
using System.Threading.Tasks;

namespace NovaLine.Event
{
    [Serializable]
    public class WaitingEvent : NovaEvent
    {
        public int ms;
        public WaitingEvent(int ms)
        {
            this.ms = ms;
        }

        public override async Task onEvent()
        {
            await Task.Delay(ms);
        }

        public override string getType()
        {
            return "Waiting Event";
        }
    }
}
