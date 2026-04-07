using System.Collections.Generic;
using System;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class RemoveEdgeCommand : Command
    {
        public List<IEdgeData> linkedDataList = new();

        public RemoveEdgeCommand(string contextGuid, NovaElementType contextType, IEdgeData linkedData) : this(contextGuid, contextType, new List<IEdgeData>() { linkedData })
        {
        }
        public RemoveEdgeCommand(string contextGuid, NovaElementType contextType, List<IEdgeData> linkedDataList) : base(contextGuid, contextType)
        {
            type = CommandType.Remove_Edge;
            this.linkedDataList.AddRange(linkedDataList);
        }
        public override void onUndo()
        {
            foreach (var linkedData in linkedDataList)
            {
                linkedGraphView.addGraphEdgeByCommand(linkedData);
            }
        }
        public override void onRedo()
        {
            foreach (var linkedData in linkedDataList)
            {
                linkedGraphView.removeGraphEdgeByCommand(linkedData);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveEdgeCommand removeEdgeCommand) return;
            linkedDataList.AddRange(removeEdgeCommand.linkedDataList);
        }
    }
}
