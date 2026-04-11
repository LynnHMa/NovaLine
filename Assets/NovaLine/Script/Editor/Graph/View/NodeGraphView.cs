﻿using NovaLine.Script.Data.Edge;
 using NovaLine.Script.Editor.Utils;
 using NovaLine.Script.Editor.Window.Context.Edge;
 using NovaLine.Script.Editor.Window.Context.GraphViewNode;
 using NovaLine.Script.Element.Switcher;
using UnityEngine;
namespace NovaLine.Script.Editor.Graph.View
{
    using NovaLine.Script.Action;
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Node;
    using NovaLine.Script.Editor.Graph.Edge;
    using NovaLine.Script.Data.NodeGraphView;

    public class NodeGraphView : NovaGraphView<ActionGraphNode,Node,NovaAction,ActionSwitcher>
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public NodeGraphView(string linkedNodeGuid) : base(linkedNodeGuid) { }
        public override ActionGraphNode summonNewGraphNode(Vector2 pos)
        {
            var actualName = (linkedElement.childrenGuidList.Count + 1).ToString();
            var newAction = new NovaAction(actualName);
            var newActionGraphNode = new ActionGraphNode(newAction, pos);
            return newActionGraphNode;
        }
        public override ActionGraphNode summonNewGraphNode(NovaAction novaAction, Vector2 pos)
        {
            return new ActionGraphNode(novaAction, pos);
        }        
        public override IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return summonAndConnectEdge<ActionGraphEdge>((ActionSwitcher)linkedSwitcher);
        }
        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return summonNewChildGraphViewNodeContext(new ActionData(linkedElement as NovaAction, pos));
        }

        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new ActionNodeContext(linkedData as ActionData);
        }
        
        public override EdgeContext summonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new ActionEdgeData(linkedSwitcher as ActionSwitcher));
        }
        protected override void updateNodes()
        {
            base.updateNodes();

            foreach(var graphNode in graphNodes.Values)
            {
                if(graphNode.linkedElement is NovaAction action && action.actionType == ActionType.Meanwhile)
                {
                    setNodePassable(graphNode);
                }
            }
        }
    }
}