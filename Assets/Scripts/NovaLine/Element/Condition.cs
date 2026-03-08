using NovaLine.Event;
using NovaLine.Switcher;
using NovaLine.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaLine.Element
{
    [Serializable]
    public class Condition : NovaElement
    {
        public static Condition DEFAULT_CONDITION = new Condition(new List<NovaEvent>() { MouseClickEvent.LEFT_CLICK }, ConditionType.All);

        public List<NovaEvent> novaEvents { get; set; } = new();
        public ConditionType type = ConditionType.All;
        private List<Task> waitingTasks;

        public Condition(List<NovaEvent> novaEvents)
        {
            guid = Guid.NewGuid().ToString();
            this.novaEvents = novaEvents;
            waitingTasks = new();
            foreach (var novaEvent in novaEvents)
            {
                waitingTasks.Add(novaEvent?.onEvent());
            }
        }
        public Condition(List<NovaEvent> novaEvents, ConditionType type) : this(novaEvents)
        {
            this.type = type;
        }
        public Condition(List<NovaEvent> novaEvents, ConditionType type,string guid)
        {
            this.type = type;
            this.novaEvents = novaEvents;
            this.guid = guid;
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
