using System.Linq;
using NovaLine.Element.Switcher;

namespace NovaLine.Action
{
    using System;
    using System.Threading.Tasks;
    using NovaLine.Element;
    using UnityEngine;

    [Serializable]
    public class NovaAction : NovaElement,INovaAction
    {
        protected Action chainedAction;

        public ActionType actionType = ActionType.Sort;

        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public override NovaElementType type => NovaElementType.ACTION;
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
        public virtual async Task invoke()
        {
            await conditionBeforeInvoke.waiting();

            chainedAction?.Invoke();

            await conditionAfterInvoke.waiting();

            var next = (NovaAction)switchers.FirstOrDefault()?.inputElement;
            if (next != null)
            {
                await next?.invoke();
            }
            else await Task.CompletedTask;
        }
        public override string getTypeName()
        {
            return "[Default Action]";
        }

        public override NovaElement copy()
        {
            var clone = base.copy();
            if (clone == null || clone is not NovaAction action) return clone;
            action.conditionBeforeInvoke = (Condition)conditionBeforeInvoke.copy();
            action.conditionAfterInvoke = (Condition)conditionAfterInvoke.copy();
            action.conditionBeforeInvoke.parent = action;
            action.conditionAfterInvoke.parent = action;
            return action;
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
