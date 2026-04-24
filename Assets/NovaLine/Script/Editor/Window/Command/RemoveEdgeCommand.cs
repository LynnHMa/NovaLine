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

        public RemoveEdgeCommand(string contextGUID, NovaElementType contextType, IEdgeData linkedData) : this(contextGUID, contextType, new List<IEdgeData>() { linkedData })
        {
        }
        public RemoveEdgeCommand(string contextGUID, NovaElementType contextType, List<IEdgeData> linkedDataList) : base(contextGUID, contextType)
        {
            Type = CommandType.Remove_Edge;
            this.linkedDataList.AddRange(linkedDataList);
        }
        public override void OnUndo()
        {
            foreach (var linkedData in linkedDataList)
            {
                ParentGraphView.AddGraphEdgeByCommand(linkedData);
            }
        }
        public override void OnRedo()
        {
            foreach (var linkedData in linkedDataList)
            {
                ParentGraphView.RemoveGraphEdgeByCommand(linkedData);
            }
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveEdgeCommand removeEdgeCommand) return;
            linkedDataList.AddRange(removeEdgeCommand.linkedDataList);
        }
    }
}
