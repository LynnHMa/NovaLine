using NovaLine.Element;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class ActionSwitcher : NovaSwitcher
    {
        public ActionSwitcher() : base()
        {
        }
        public ActionSwitcher(NovaElement inputElement, NovaElement outputElement, string guid) : base(inputElement, outputElement, guid)
        {
        }
    }
}
