using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class EventSwitcher : NovaSwitcher
    {
        public EventSwitcher() : base()
        {
        }
        public override string getTypeName()
        {
            return "[Event Edge]";
        }
    }
}
