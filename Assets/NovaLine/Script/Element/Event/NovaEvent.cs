using System;
using System.Collections;
using System.Linq;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Ext;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element.Event
{
    [Serializable]
    public class NovaEvent : NovaElement,INovaEvent
    {
        public override Color ThemedColor => ColorExt.EVENT_THEMED_COLOR;
        public override NovaElementType Type => NovaElementType.Event;

        public NovaEvent()
        {
        }
        public NovaEvent(string name)
        {
            this.name = name;
        }
        public virtual IEnumerator OnEvent()
        {
            if (Parent is Condition parentCondition && parentCondition.ConditionType == ConditionType.Sort)
            {
                var firstSwitcherGuid = SwitchersGuidList.FirstOrDefault();
                if (FindElement(firstSwitcherGuid) is EventSwitcher firstSwitcher)
                {
                    yield return firstSwitcher.Next();
                }
            }
            yield return null;
        }

        public override string GetTypeName()
        {
            return "[Default Event]";
        }
    }
    public interface INovaEvent
    {
        IEnumerator OnEvent();
    }
}
