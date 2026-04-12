using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class ActionSwitcher : NovaSwitcher
    {
        public override string GetTypeName()
        {
            return "[Action Edge]";
        }

        public override IEnumerator Next()
        {
            var nextAction = tryToFindInputElement();
            if (nextAction is NovaAction action)
            {
                yield return action.Invoke();
            }
            
            yield return base.Next();
        }
    }
}
