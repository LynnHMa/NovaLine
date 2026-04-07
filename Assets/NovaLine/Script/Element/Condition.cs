using NovaLine.Script.Utils.Ext;
using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Element.Event;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Condition : NovaElement
    {
        public ConditionType conditionType = ConditionType.All;
        public override NovaElementType type => NovaElementType.CONDITION;

        public Condition()
        {
        }
        public Condition(string name,NovaElement parent)
        {
            this.name = name;
            setParent(parent);
        }
        public override string getTypeName()
        {
            return "[Condition]";
        }
        public IEnumerator waiting()
        {
            var routines = getRoutines();
            switch (conditionType)
            {
                case ConditionType.All:
                    yield return routines.WhenAll();
                    break;
                case ConditionType.Any:
                    yield return routines.WhenAny();
                    break;
                case ConditionType.Sort:
                    var firstEvent = firstChild as NovaEvent;
                    yield return firstEvent?.onEvent();
                    break;
            }

            yield return null;
        }
        
        private List<IEnumerator> getRoutines()
        {
            List<IEnumerator> routines = new();
            foreach(var childGuid in childrenGuidList)
            {
                var child = FindElement(childGuid);
                if (child is not NovaEvent novaEvent) continue;
                routines.Add(novaEvent.onEvent());
            }
            return routines;
        }

        public override void setParent(NovaElement parent)
        {
            parentGuid = parent.guid;
        }
    }
    public enum ConditionType
    {
        All,
        Any,
        Sort
    }
}
