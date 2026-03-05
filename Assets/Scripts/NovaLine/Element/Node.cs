using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Action;
using NovaLine.Switcher;
using NovaLine.Interface;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Node : NovaElement
    {
        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public List<NodeSwitcher> nextNodes { get; set; } = new();

        public List<NovaAction> actions = new();

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
            for (int i = 0; i < actions.Count; i++)
            {
                await actions[i]?.invoke();
            }
            await conditionAfterInvoke.waiting();
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if(graphEdge is NodeSwitcher nodeSwitcher && parent is Flowchart parentFlowchart)
            {
                if (nextNodes == null)
                {
                    nextNodes = new List<NodeSwitcher>();
                }
                nextNodes.Add(nodeSwitcher);
                var outputNode = nodeSwitcher.outputElement as Node;
                var inputNode = nodeSwitcher.inputElement as Node;
                if (outputNode != null && inputNode != null)
                {
                    var oIndex = parentFlowchart.nodes.FindIndex(e => e.guid == outputNode.guid);
                    var iIndex = oIndex + 1;
                    if (iIndex >= 0)
                    {
                        parentFlowchart?.nodes?.Insert(iIndex, inputNode);
                    }
                }
            }
        }
        public override void onGraphDisconnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is NodeSwitcher nodeSwitcher && parent is Flowchart parentFlowchart)
            {
                if (nextNodes == null)
                {
                    nextNodes = new List<NodeSwitcher>();
                }
                nextNodes.Remove(nodeSwitcher);
                var outputNode = nodeSwitcher.outputElement as Node;
                var inputNode = nodeSwitcher.inputElement as Node;
                if (outputNode != null && inputNode != null)
                {
                    var oIndex = parentFlowchart.nodes.FindIndex(e => e.guid == outputNode.guid);
                    var iIndex = oIndex + 1;
                    if (iIndex >= 0)
                    {
                        parentFlowchart?.nodes?.RemoveAt(iIndex);
                    }
                }
            }
        }
    }
}
