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

        public AddNodeCommand(string contextGUID, NovaElementType contextType, IGraphViewNodeData linkedData) : this(contextGUID, contextType, new List<IGraphViewNodeData>() { linkedData })
        {
        }
        public AddNodeCommand(string contextGUID, NovaElementType contextType, List<IGraphViewNodeData> linkedDataList) : base(contextGUID, contextType)
        {
            Type = CommandType.Add_Node;
            this.linkedDataList.AddRange(linkedDataList);
        }

        public override void OnUndo()
        {
            foreach(var linkedData in linkedDataList)
            {
                ParentGraphView.RemoveGraphNodeByCommand(linkedData);
            }
        }
        public override void OnRedo()
        {
            foreach (var linkedData in linkedDataList)
            {
                ParentGraphView.AddGraphNodeByCommand(linkedData);
            }
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not AddNodeCommand addNodeCommand) return;
            linkedDataList.AddRange(addNodeCommand.linkedDataList);
        }
    }
}
