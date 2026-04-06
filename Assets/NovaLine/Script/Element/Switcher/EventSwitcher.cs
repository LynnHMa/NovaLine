using System;

namespace NovaLine.Script.Element.Switcher
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
