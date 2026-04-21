using NovaLine.Script.Utils.Ext;
using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Element.Event;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Condition : NovaElement
    {
        public ConditionType ConditionType = ConditionType.All;
        public override Color ThemedColor => ColorExt.CONDITION_THEMED_COLOR;
        public override NovaElementType Type => NovaElementType.Condition;

        public Condition()
        {
        }
        public Condition(string name,NovaElement parent)
        {
            this.name = name;
            SetParent(parent);
        }
        public override string GetTypeName()
        {
            return "[Condition]";
        }
        public IEnumerator Waiting()
        {
            var routines = GetRoutines();
            switch (ConditionType)
            {
                case ConditionType.All:
                    yield return routines.WhenAll();
                    break;
                case ConditionType.Any:
                    yield return routines.WhenAny();
                    break;
                case ConditionType.Sort:
                    var firstEvent = FirstChild as NovaEvent;
                    yield return firstEvent?.OnEvent();
                    break;
            }

            yield return null;
        }
        
        private List<IEnumerator> GetRoutines()
        {
            List<IEnumerator> routines = new();
            foreach(var childGuid in ChildrenGuidList)
            {
                var child = FindElement(childGuid);
                if (child is not NovaEvent novaEvent) continue;
                routines.Add(novaEvent.OnEvent());
            }
            return routines;
        }

        public override void SetParent(NovaElement parent)
        {
            ParentGuid = parent != null ? parent.Guid : "";
        }
    }
    public enum ConditionType
    {
        All,
        Any,
        Sort
    }
}
