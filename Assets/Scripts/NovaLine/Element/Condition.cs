using NovaLine.Event;
using NovaLine.Switcher;
using NovaLine.Utils.Ext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Condition : NovaElement
    {
        public ConditionType type = ConditionType.All;

        [HideInInspector]
        public List<NovaEvent> novaEvents = new();

        [HideInInspector]
        public NovaEvent firstEvent;
        public Condition()
        {
            guid = Guid.NewGuid().ToString();
        }
        public Condition(NovaElement parent) : this()
        {
            this.parent = parent;
        }
        public override string getType()
        {
            return "[Condition]";
        }
        public async Task waiting()
        {
            var waitingTasks = getWaitingTasks();
            switch (type)
            {
                case ConditionType.All:
                    await waitingTasks.RunAll();
                    break;
                case ConditionType.Any:
                    await waitingTasks.RunAny();
                    break;
                case ConditionType.Sort:
                    await firstEvent?.onEvent();
                    break;
            }
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            //There is not condition graph node.
        }
        private List<Task> getWaitingTasks()
        {
            List<Task> waitingTasks = new();
            foreach(var novaEvent in novaEvents)
            {
                if (novaEvent == null) continue;
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
