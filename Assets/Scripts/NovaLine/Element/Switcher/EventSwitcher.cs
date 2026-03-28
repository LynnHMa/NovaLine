using System;

namespace NovaLine.Element.Switcher
{
    [Serializable]
    public class EventSwitcher : NovaSwitcher
    {
        public override string getTypeName()
        {
            return "[Event Edge]";
        }
    }
}
