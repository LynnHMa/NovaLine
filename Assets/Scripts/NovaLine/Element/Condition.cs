using NovaLine.Event;
using NovaLine.Switcher;
using NovaLine.Utils;
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
        public EList<NovaEvent> novaEvents = new();

        [HideInInspector]
        public NovaEvent firstEvent;

        private List<Task> waitingTasks = new();
        public Condition()
        {
            guid = Guid.NewGuid().ToString();
        }
        public Condition(INovaElement parent) : this()
        {
            this.parent = parent;
        }
        public override string getType()
        {
            return "[Condition]";
        }
        public async Task waiting()
        {
            switch (type)
            {
                case ConditionType.All:
                    await waitingTasks.RunAll();
                    break;
                case ConditionType.Any:
                    await waitingTasks.RunAny();
                    break;
                case ConditionType.Sort:
                    //todo next Event
                    foreach (var novaEvent in novaEvents)
                    {
                        await novaEvent?.onEvent();
                    }
                    break;
            }
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
        }
    }
    public enum ConditionType
    {
        All,
        Any,
        Sort
    }
}
