using System;
using NovaLine.Switcher;

namespace NovaLine.Element.Switcher
{
    [Serializable]
    public class ActionSwitcher : NovaSwitcher
    {
        public ActionSwitcher() : base()
        {
        }
        public override string getTypeName()
        {
            return "[Action Edge]";
        }
    }
}
