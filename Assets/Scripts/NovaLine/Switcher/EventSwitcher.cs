using NovaLine.Element;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class EventSwitcher : NovaSwitcher
    {
        public EventSwitcher() : base()
        {
        }
        public EventSwitcher(NovaElement inputElement, NovaElement outputElement, string guid) : base(inputElement, outputElement, guid)
        {
        }
    }
}
