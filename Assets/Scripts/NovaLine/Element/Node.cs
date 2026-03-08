using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Action;
using NovaLine.Switcher;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Node : NovaElement
    {
        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public List<NodeSwitcher> nextNodes  = new();

        public List<NovaAction> actions = new();

        public NovaAction firstAction { get; set; }

        public Node() {
            guid = Guid.NewGuid().ToString();
        }

        public Node(Node node)
        {
            name = node.name;
            describtion = node.describtion;
            conditionAfterInvoke = node.conditionAfterInvoke;
            conditionBeforeInvoke = node.conditionBeforeInvoke;
            actions = node.actions;
            guid = node.guid;
        }

        public Node(string name) : this()
        {
            this.name = name;
        }
        public Node(string name, string describtion, Condition conditionBeforeInvoke, Condition conditionAfterInvoke, List<NovaAction> actions,string guid)
        {
            this.name = name;
            this.describtion = describtion;
            this.conditionBeforeInvoke = conditionBeforeInvoke;
            this.conditionAfterInvoke = conditionAfterInvoke;
            this.actions = actions;
            this.guid = guid;
        }

        public async Task run()
        {
            await conditionBeforeInvoke.waiting();
            foreach (var action in actions)
            {
                if(action.type == ActionType.Meanwhile) action.invoke();
            }
            await (firstAction == null ? Task.CompletedTask : firstAction.invoke());
            await conditionAfterInvoke.waiting();
        }
        public override string getType()
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
