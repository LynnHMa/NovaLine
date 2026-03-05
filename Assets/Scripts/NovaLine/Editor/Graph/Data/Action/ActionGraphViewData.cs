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
        public NovaAction action;

        public override string name => action == null ? "Action" : action.getType() + " Action";

        public override string describtion => action == null ? "Action is null." : action.getDescribation();
        public ActionGraphViewData(NovaAction action,Vector2 pos)
        {
            this.action = action;
            this.pos = pos;
            guid = action?.guid;
        }
        public override NovaAction to()
        {
            return base.to();
        }
        public override void draw(INovaGraphView graphView)
        {
            //在Node编辑界面中绘制本体（Action）节点
            if (graphView != null && graphView is NodeGraphView nodeGraphView)
            {
                var graphNode = new ActionGraphNode(action,pos);
                Debug.Log("pos in" + pos);
                nodeGraphView.addGraphNode(graphNode,true);
            }

            //由于没有Action编辑界面，所以不在其他界面中绘制Action节点
        }
    }
}
