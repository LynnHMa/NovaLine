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
        protected override Color ThemedColor => ColorExt.ACTION_THEMED_COLOR;
        public NodeGraphView(string linkedNodeGuid) : base(linkedNodeGuid) { }
        public override ActionGraphNode SummonNewGraphNode(Vector2 pos)
        {
            var actualName = (LinkedElement.ChildrenGuidList.Count + 1).ToString();
            var newAction = new NovaAction(actualName);
            var newActionGraphNode = new ActionGraphNode(newAction, pos);
            return newActionGraphNode;
        }
        public override ActionGraphNode SummonNewGraphNode(NovaAction novaAction, Vector2 pos)
        {
            return new ActionGraphNode(novaAction, pos);
        }        
        public override IGraphEdge SummonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return SummonAndConnectEdge<ActionGraphEdge>((ActionSwitcher)linkedSwitcher);
        }
        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return SummonNewChildGraphViewNodeContext(new ActionData(linkedElement as NovaAction, pos));
        }

        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new ActionNodeContext(linkedData as ActionData);
        }
        
        public override EdgeContext SummonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new ActionEdgeData(linkedSwitcher as ActionSwitcher));
        }
        protected override void UpdateNodes()
        {
            base.UpdateNodes();

            foreach(var graphNode in GraphNodes.Values)
            {
                if(graphNode.LinkedElement is NovaAction action && action.ActionType == ActionType.Meanwhile)
                {
                    SetNodePassable(graphNode);
                }
            }
        }
    }
}