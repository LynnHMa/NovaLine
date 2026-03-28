using System;

namespace NovaLine.Element.Switcher
{
    [Serializable]
    public class ActionSwitcher : NovaSwitcher
    {
        public override string getTypeName()
        {
            return "[Action Edge]";
        }
    }
}
