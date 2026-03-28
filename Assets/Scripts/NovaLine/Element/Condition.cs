using NovaLine.Utils.Ext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Element.Event;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Condition : NovaElement
    {
        public ConditionType conditionType = ConditionType.All;
        public override NovaElementType type => NovaElementType.CONDITION;
        public Condition()
        {
            guid = Guid.NewGuid().ToString();
        }
        public Condition(NovaElement parent) : this()
        {
            this.parent = parent;
        }
        public override string getTypeName()
        {
            return "[Condition]";
        }
        public async Task waiting()
        {
            var waitingTasks = getWaitingTasks();
            switch (conditionType)
            {
                case ConditionType.All:
                    await waitingTasks.RunAll();
                    break;
                case ConditionType.Any:
                    await waitingTasks.RunAny();
                    break;
                case ConditionType.Sort:
                    var firstEvent = (NovaEvent)firstChild;
                    await firstEvent.onEvent();
                    break;
            }
        }
        
        private List<Task> getWaitingTasks()
        {
            List<Task> waitingTasks = new();
            foreach(var child in children)
            {
                if (child == null || child is not NovaEvent novaEvent) continue;
                waitingTasks.Add(novaEvent.onEvent());
            }
            return waitingTasks;
        }
    }
    public enum ConditionType
    {
        All,
        Any,
        Sort
    }
}
