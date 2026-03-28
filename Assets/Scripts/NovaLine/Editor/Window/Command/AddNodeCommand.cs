using NovaLine.Editor.Window.Context;
using UnityEngine;
using Editor.Utils.Ext;
using System.Collections.Generic;
using NovaLine.Editor.Graph.Node;
using System;
using static NovaLine.Editor.Window.WindowContextRegistry;
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

        protected override void onUndo()
        {
            foreach(var graphNodeInfo in addedGraphNodeInfo)
            {
                linkedGraphView.removeGraphNodeByCommand(graphNodeInfo.key);
            }
        }
        protected override void onRedo()
        {
            foreach (var graphNodeInfo in addedGraphNodeInfo)
            {
                linkedGraphView.addGraphNodeByCommand(graphNodeInfo.key,graphNodeInfo.value);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddNodeCommand addNodeCommand) return;
            addedGraphNodeInfo.AddRange(addNodeCommand.addedGraphNodeInfo);
        }
    }
}
