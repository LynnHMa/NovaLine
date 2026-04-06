using System;
using System.Collections;
using System.Linq;
using NovaLine.Script.Element.Switcher;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
        public override NovaElementType type => NovaElementType.EVENT;

        public NovaEvent()
        {
        }
        public NovaEvent(string name)
        {
            this.name = name;
        }
        public virtual IEnumerator onEvent()
        {
            if (parent is Condition parentCondition && parentCondition.conditionType == ConditionType.Sort)
            {
                var firstSwitcherGuid = switchersGuidList.FirstOrDefault();
                if (FindElement(firstSwitcherGuid) is not EventSwitcher firstSwitcher) yield break;
                var nextElement = firstSwitcher.tryToFindInputElement();
                if (nextElement is INovaEvent nextEvent)
                {
                    yield return nextEvent.onEvent();
                }
            }
            else yield return null;
        }

        public override string getTypeName()
        {
            return "[Default Event]";
        }
    }
    public interface INovaEvent
    {
        IEnumerator onEvent();
    }
}
