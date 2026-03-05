using System;
using System.Collections.Generic;

namespace NovaLine.Editor.Graph.Data
{
    using NovaLine.Action;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Editor.Graph.View;
    using UnityEngine;

    [Serializable]
    public class NodeGraphViewData : GraphViewNodeData<Node>
    {
        public List<ActionGraphViewData> actions { get; set; } = new();

        public List<ActionEdgeGraphViewData> actionEdgeGraphViewData { get; set; } = new();

        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;
        public NodeGraphViewData(Node node, Vector2 pos)
        {
            this.pos = pos;
            guid = node.guid;
            name = node.name;
            conditionBeforeInvoke = node.conditionBeforeInvoke;
            conditionAfterInvoke = node.conditionAfterInvoke;
            describtion = node.describtion;
            for (var i = 0; i < node.actions?.Count; i++)
            {
                var action = node.actions?[i];

                //这里不生成Action节点，所以位置默认为0
                var actionData = new ActionGraphViewData(action,Vector2.zero);
                actions.Add(actionData);
            }
        }
        public NodeGraphViewData(NodeGraphView nodeGraphView,Vector2 pos)
        {
            this.pos = pos;
            var node = nodeGraphView.root;
            guid = node.guid;
            name = node.name;
            conditionBeforeInvoke = node.conditionBeforeInvoke;
            conditionAfterInvoke = node.conditionAfterInvoke;
            describtion = node.describtion;
            for (var i = 0; i < nodeGraphView.graphNodes?.Count; i++)
            {
                var actionGraphNode = nodeGraphView.graphNodes?[i];
                var actionData = new ActionGraphViewData((NovaAction)actionGraphNode.targetObject, actionGraphNode.pos);
                actions.Add(actionData);
            }
        }
        public override Node to()
        {
            var node = new Node(name, describtion, conditionBeforeInvoke, conditionAfterInvoke, getNovaActions(), guid);
            for(var i = 0;i < node.actions.Count - 1; i++)
            {
                actionEdgeGraphViewData.Add(new ActionEdgeGraphViewData(new Switcher.ActionSwitcher()));
            }
            return node;
        }
        public override void draw(INovaGraphView graphView)
        {
            //在Flowchart编辑界面中绘制本体（Node）节点
            if (graphView != null && graphView is FlowchartGraphView flowchartGraphView)
            {
                var node = new Node(name, describtion, conditionBeforeInvoke, conditionAfterInvoke, getNovaActions(), guid);
                var graphNode = new NodeGraphNode(node,pos);
                flowchartGraphView.addGraphNode(graphNode,true);
            }

            //在Node编辑界面中绘制Action节点
            else if (graphView is NodeGraphView nodeGraphView)
            {
                //节点本体
                foreach(var actionData in actions)
                {
                    actionData.draw(graphView);
                }

                //绘制Action节点间的连线
                for (int i = actionEdgeGraphViewData.Count - 1; i >= 0; i--)
                {
                    var actionSwitcherData = actionEdgeGraphViewData[i];
                    actionSwitcherData.draw(this,graphView);
                }
            }
        }
        private List<NovaAction> getNovaActions()
        {
            var toActions = new List<NovaAction>();
            for (var i = 0; i < actions.Count; i++)
            {
                var actionData = actions[i];
                var action = actionData.to();
                toActions.Add(action);
            }
            return toActions;
        }
    }
}
