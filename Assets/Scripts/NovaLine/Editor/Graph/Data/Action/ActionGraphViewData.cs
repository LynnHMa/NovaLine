using System;

namespace NovaLine.Editor.Graph.Data
{
    using NovaLine.Action;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Editor.Graph.View;
    using UnityEngine;

    [Serializable]
    public class ActionGraphViewData : GraphViewNodeData<NovaAction>
    {
        public ActionGraphViewData(NovaAction linkedAction, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = linkedAction;
        }
        public override void draw(INovaGraphView graphView)
        {
            //在Node编辑界面中绘制本体（Action）节点
            if (graphView != null && graphView is NodeGraphView nodeGraphView)
            {
                var graphNode = new ActionGraphNode(linkedElement, pos);
                nodeGraphView.addGraphNode(graphNode,true,false);

                if (graphNode.guid.Equals(startGraphNodeGuid))
                {
                    nodeGraphView.firstNode = graphNode;
                }
            }

            //由于没有Action编辑界面，所以不在其他界面中绘制Action节点
        }
    }
}
