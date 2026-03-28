using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Action;
using NovaLine.Element.Switcher;

namespace NovaLine.Element
{
    [Serializable]
    public class Node : NovaElement
    {
        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public override NovaElementType type => NovaElementType.NODE;
        public Node() {
            guid = Guid.NewGuid().ToString();
        }

        public Node(string name) : this()
        {
            this.name = name;
            conditionBeforeInvoke = new(this);
            conditionAfterInvoke = new(this);
        }

        public async Task run()
        {
            await conditionBeforeInvoke.waiting();
            foreach (var child in children)
            {
                if(child == null || child is not NovaAction action) continue;
                if(action.actionType == ActionType.Meanwhile) action?.invoke();
            }
            await (firstChild == null || firstChild is not NovaAction firstAction ? Task.CompletedTask : firstAction.invoke());
            await conditionAfterInvoke.waiting();
        }
        public override string getTypeName()
        {
            return "[Node]";
        }
        public override NovaElement copy()
        {
            var clone = base.copy();
            if (clone == null || clone is not Node node) return clone;
            node.conditionBeforeInvoke = (Condition)conditionBeforeInvoke.copy();
            node.conditionAfterInvoke = (Condition)conditionAfterInvoke.copy();
            node.conditionBeforeInvoke.parent = node;
            node.conditionAfterInvoke.parent = node;
            return node;
        }
        private List<Task> getNextNodeConditionTasks()
        {
            var result = new List<Task>();
            foreach (var switcher in switchers)
            {
                if (switcher is NodeSwitcher nodeSwitcher)
                {
                    result.Add(nodeSwitcher.switchCondition?.waiting());
                }
            }
            return result;
        }
    }
}
