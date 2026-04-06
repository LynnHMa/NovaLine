using UnityEngine;
using Editor.Utils.Ext;
using System.Collections.Generic;
using NovaLine.Script.Editor.Graph.Node;
using System;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class RemoveNodeCommand : Command
    {
        public List<KeyValue<string,Vector2>> removedGraphNodeInfo = new();

        public RemoveNodeCommand(string contextGuid, NovaElementType contextType, GraphNode graphNode) : this(contextGuid, contextType, new List<GraphNode>() { graphNode })
        {
        }
        public RemoveNodeCommand(string contextGuid, NovaElementType contextType, List<GraphNode> graphNodes) : base(contextGuid, contextType)
        {
            type = CommandType.Remove_Node;
            foreach (var graphNode in graphNodes)
            {
                removedGraphNodeInfo.Add(new(graphNode.linkedElementGuid, graphNode.pos));
            }
        }
        public override void onUndo()
        {
            foreach (var graphNodeInfo in removedGraphNodeInfo)
            {
                linkedGraphView.addGraphNodeByCommand(graphNodeInfo.key,graphNodeInfo.value);
            }
        }
        public override void onRedo()
        {
            foreach(var graphNodeInfo in removedGraphNodeInfo)
            {
                linkedGraphView.removeGraphNodeByCommand(graphNodeInfo.key);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveNodeCommand removeNodeCommand) return;
            removedGraphNodeInfo.AddRange(removeNodeCommand.removedGraphNodeInfo);
        }
    }
}
