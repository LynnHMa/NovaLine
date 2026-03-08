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
        public List<ActionGraphViewData> actionGraphViewDatas { get; set; } = new();
        public List<ActionEdgeGraphViewData> actionEdgeGraphDatas { get; set; } = new();
        public NodeGraphViewData(Node node, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = node;
            startGraphNodeGuid = linkedElement.firstAction?.guid;

            for (var i = 0; i < node.actions?.Count; i++)
            {
                var action = node.actions?[i];

                //这里不生成Action节点，所以位置默认为0
                var actionData = new ActionGraphViewData(action, Vector2.zero);
                actionGraphViewDatas.Add(actionData);
            }
        }
        public NodeGraphViewData(NodeGraphView nodeGraphView,Vector2 pos)
        {
            this.pos = pos;
            linkedElement = nodeGraphView.root;
            startGraphNodeGuid = linkedElement.firstAction?.guid;

            for (var i = 0; i < nodeGraphView.graphNodes?.Count; i++)
            {
                var actionGraphNode = nodeGraphView.graphNodes?[i];

                var linkedAction = (NovaAction)actionGraphNode.linkedElement;
                if (linkedAction == null) continue;
                var actionData = new ActionGraphViewData(linkedAction, actionGraphNode.pos);
                actionGraphViewDatas.Add(actionData);

                var linkedActionSwitcher = linkedAction.nextAction;
                if (linkedActionSwitcher == null) continue;
                var actionEdgeData = new ActionEdgeGraphViewData(linkedActionSwitcher);
                actionEdgeGraphDatas.Add(actionEdgeData);
            }
        }
        public override void draw(INovaGraphView graphView)
        {
            //在Flowchart编辑界面中绘制本体（Node）节点
            if (graphView != null && graphView is FlowchartGraphView flowchartGraphView)
            {
                var node = linkedElement;
                var graphNode = new NodeGraphNode(node,pos);
                flowchartGraphView.addGraphNode(graphNode,true,false);
                if (graphNode.guid.Equals(startGraphNodeGuid))
                {
                    flowchartGraphView.firstNode = graphNode;
                }
            }

            //在Node编辑界面中绘制Action节点
            else if (graphView is NodeGraphView nodeGraphView)
            {
                //节点本体
                for (int i = 0; i < actionGraphViewDatas.Count; i++)
                {
                    var actionData = actionGraphViewDatas[i];
                    actionData.draw(nodeGraphView);
                }

                //更新节点的Port，防止连线的时候找不到有效Port
                graphView.updateAllGraphElements();

                //绘制Action节点间的连线
                for (int i = 0; i < actionEdgeGraphDatas.Count; i++)
                {
                    var actionEdgeData = actionEdgeGraphDatas[i];
                    if(actionEdgeData == null)
                    {
                        continue;
                    }
                    actionEdgeData.draw(this,nodeGraphView);
                }
            }
        }
        private List<NovaAction> getNovaActions()
        {
            var toActions = new List<NovaAction>();
            for (var i = 0; i < actionGraphViewDatas.Count; i++)
            {
                var actionData = actionGraphViewDatas[i];
                var action = actionData.linkedElement;
                toActions.Add(action);
            }
            return toActions;
        }
    }
}
