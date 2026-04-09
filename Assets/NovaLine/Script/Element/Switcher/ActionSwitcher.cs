using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class ActionSwitcher : NovaSwitcher
    {
        public override string getTypeName()
        {
            return "[Action Edge]";
        }

        public override IEnumerator next()
        {
            var nextAction = tryToFindInputElement();
            if (nextAction is NovaAction action)
            {
                yield return action.invoke();
            }
            
            yield return base.next();
        }
    }
}
