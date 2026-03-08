namespace NovaLine.Action
{
    using System;
    using System.Threading.Tasks;
    using NovaLine.Element;
    using NovaLine.Switcher;

    [Serializable]
    public class NovaAction : NovaElement,INovaAction
    {
        public static NovaAction EMPTY_ACTION = new();

        protected Action chainedAction;

        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public ActionSwitcher nextAction;

        public ActionType type = ActionType.Sort;

        public NovaAction()
        {
            guid = Guid.NewGuid().ToString();
        }

        public NovaAction(string name) : this()
        {
            this.name = name;
        }

        public NovaAction(string name,string guid) : this(name)
        {
            this.guid = guid;
        }

        public override string getType()
        {
            return "[Default Action]";
        }

        public virtual async Task invoke()
        {
            await conditionBeforeInvoke.waiting();
            chainedAction?.Invoke();
            await conditionAfterInvoke.waiting();
            var next = (NovaAction)nextAction?.outputElement;
            if (next != null)
            {
                await next.invoke();
            }
            else await Task.CompletedTask;
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is ActionSwitcher actionSwitcher)
            {
                nextAction = actionSwitcher;
            }
        }
        public override void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            nextAction = null;
        }
    }
    public interface INovaAction
    {

    }
}
public enum ActionType
{
    Meanwhile,
    Sort
}
