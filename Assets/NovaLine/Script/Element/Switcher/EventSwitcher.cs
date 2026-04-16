using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Element.Event;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class EventSwitcher : NovaSwitcher
    {
        public override string GetTypeName()
        {
            return "[Event Edge]";
        }
        
        public override IEnumerator Next()
        {
            var nextAction = TryToFindInputElement();
            if (nextAction is NovaEvent novaEvent)
            {
                yield return novaEvent.OnEvent();
            }
            
            yield return base.Next();
        }
    }
}
