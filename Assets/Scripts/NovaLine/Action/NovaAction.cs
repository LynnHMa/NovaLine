namespace NovaLine.Action
{
    using System;
    using System.Threading.Tasks;
    using NovaLine.Element;
    using NovaLine.Switcher;
    using UnityEngine;

    [Serializable]
    public class NovaAction : NovaElement,INovaAction
    {
        protected Action chainedAction;

        public ActionType type = ActionType.Sort;

        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        [HideInInspector]
        public ActionSwitcher nextAction;

        public NovaAction()
        {
            guid = Guid.NewGuid().ToString();
            conditionBeforeInvoke = new(this);
            conditionAfterInvoke = new(this);
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

            var next = (NovaAction)nextAction?.inputElement;
            if (next != null)
            {
                await next?.invoke();
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
