using UnityEngine;
using Editor.Utils.Ext;
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
            type = CommandType.Add_Node;
            this.linkedDataList.AddRange(linkedDataList);
        }

        public override void onUndo()
        {
            foreach(var linkedData in linkedDataList)
            {
                linkedGraphView.removeGraphNodeByCommand(linkedData);
            }
        }
        public override void onRedo()
        {
            foreach (var linkedData in linkedDataList)
            {
                linkedGraphView.addGraphNodeByCommand(linkedData);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddNodeCommand addNodeCommand) return;
            linkedDataList.AddRange(addNodeCommand.linkedDataList);
        }
    }
}
