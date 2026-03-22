using NovaLine.Editor.Window.Context;
using UnityEngine;
using Editor.Utils.Ext;
using System.Collections.Generic;
using NovaLine.Editor.Graph.Node;
using System;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class AddNodeCommand : Command
    {
        public List<KeyValue<NovaElement,Vector2>> addedGraphNodeInfo = new();

        public AddNodeCommand(string contextGuid, NovaElementType contextType, GraphNode graphNode) : this(contextGuid, contextType, new List<GraphNode>() { graphNode })
        {
        }
        public AddNodeCommand(string contextGuid, NovaElementType contextType, List<GraphNode> graphNodes) : base(contextGuid, contextType)
        {
            type = CommandType.Add_Node;
            foreach (var graphNode in graphNodes)
            {
                addedGraphNodeInfo.Add(new(graphNode.linkedElement, graphNode.pos));
            }
        }

        public override void undo(bool autoSave = false)
        {
            if(linkedGraphView != null)
            {
                foreach(var graphNodeInfo in addedGraphNodeInfo)
                {
                    linkedGraphView.removeGraphNode(graphNodeInfo.key?.guid, false, false);
                }
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = false)
        {
            if (linkedGraphView != null)
            {
                foreach (var graphNodeInfo in addedGraphNodeInfo)
                {
                    linkedGraphView.addGraphNode(linkedGraphView.summonNewGraphNode(graphNodeInfo.key,graphNodeInfo.value), false, false, false);
                }
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddNodeCommand addNodeCommand) return;
            addedGraphNodeInfo.AddRange(addNodeCommand.addedGraphNodeInfo);
        }
    }
}
