using UnityEngine;
using Editor.Utils.Ext;
using System.Collections.Generic;
using NovaLine.Editor.Graph.Node;
using System;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class RemoveNodeCommand : Command
    {
        public List<KeyValue<NovaElement,Vector2>> removedGraphNodeInfo = new();

        public RemoveNodeCommand(string contextGuid, NovaElementType contextType, GraphNode graphNode) : this(contextGuid, contextType, new List<GraphNode>() { graphNode })
        {
        }
        public RemoveNodeCommand(string contextGuid, NovaElementType contextType, List<GraphNode> graphNodes) : base(contextGuid, contextType)
        {
            type = CommandType.Remove_Node;
            foreach (var graphNode in graphNodes)
            {
                removedGraphNodeInfo.Add(new(graphNode.linkedElement, graphNode.pos));
            }
        }
        public override void undo(bool autoSave = true)
        {
            if (linkedGraphView != null)
            {
                foreach (var graphNodeInfo in removedGraphNodeInfo)
                {
                    linkedGraphView.addGraphNode(linkedGraphView.summonNewGraphNode(graphNodeInfo.key, graphNodeInfo.value), false, false, false);
                }
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = true)
        {
            if(linkedGraphView != null)
            {
                foreach(var graphNodeInfo in removedGraphNodeInfo)
                {
                    linkedGraphView.removeGraphNode(graphNodeInfo.key?.guid, false, false);
                }
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveNodeCommand removeNodeCommand) return;
            removedGraphNodeInfo.AddRange(removeNodeCommand.removedGraphNodeInfo);
        }
    }
}
