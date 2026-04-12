using UnityEngine;
using System.Collections.Generic;
using NovaLine.Script.Editor.Graph.Node;
using System;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class AddNodeCommand : Command
    {
        public List<IGraphViewNodeData> linkedDataList = new();

        public AddNodeCommand(string contextGuid, NovaElementType contextType, IGraphViewNodeData linkedData) : this(contextGuid, contextType, new List<IGraphViewNodeData>() { linkedData })
        {
        }
        public AddNodeCommand(string contextGuid, NovaElementType contextType, List<IGraphViewNodeData> linkedDataList) : base(contextGuid, contextType)
        {
            Type = CommandType.Add_Node;
            this.linkedDataList.AddRange(linkedDataList);
        }

        public override void OnUndo()
        {
            foreach(var linkedData in linkedDataList)
            {
                linkedGraphView.RemoveGraphNodeByCommand(linkedData);
            }
        }
        public override void OnRedo()
        {
            foreach (var linkedData in linkedDataList)
            {
                linkedGraphView.AddGraphNodeByCommand(linkedData);
            }
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not AddNodeCommand addNodeCommand) return;
            linkedDataList.AddRange(addNodeCommand.linkedDataList);
        }
    }
}
