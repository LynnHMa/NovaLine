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
        public override string getTypeName()
        {
            return "[Event Edge]";
        }
        
        public override IEnumerator next()
        {
            var nextAction = tryToFindInputElement();
            if (nextAction is NovaEvent novaEvent)
            {
                yield return novaEvent.onEvent();
            }
            
            yield return base.next();
        }
    }
}
