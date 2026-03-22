using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Action;
using NovaLine.Switcher;
using NovaLine.Utils.Ext;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Node : NovaElement
    {
        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public override NovaElementType type => NovaElementType.NODE;

        [HideInInspector]
        public List<NodeSwitcher> nextNodes  = new();

        [HideInInspector]
        public List<NovaAction> actions = new();

        [HideInInspector]
        public NovaAction firstAction;

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
            foreach (var action in actions)
            {
                if(action.actionType == ActionType.Meanwhile) action.invoke();
            }
            await (firstAction == null ? Task.CompletedTask : firstAction.invoke());
            await conditionAfterInvoke.waiting();
        }
        public override string getTypeName()
        {
            return "[Node]";
        }
        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is NodeSwitcher nodeSwitcher)
            {
                if (nextNodes == null) nextNodes = new();
                if (!nextNodes.Exists(s => s.guid == nodeSwitcher.guid))
                {
                    nextNodes.Add(nodeSwitcher);
                }
            }
        }
        public override void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is NodeSwitcher nodeSwitcher)
            {
                if (nextNodes == null)
                {
                    nextNodes = new();
                }
                nextNodes.Remove(nodeSwitcher);
            }
        }
        private List<Task> getNextNodeConditionTasks()
        {
            var result = new List<Task>();
            foreach (var switcher in nextNodes)
            {
                result.Add(switcher.switchCondition?.waiting());
            }
            return result;
        }
    }
}
