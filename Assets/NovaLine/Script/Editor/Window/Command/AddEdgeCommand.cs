using System.Collections.Generic;
using System;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Switcher;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class AddEdgeCommand : Command
    {
        public List<IEdgeData> linkedDataList = new();

        public AddEdgeCommand(string contextGuid, NovaElementType contextType, IEdgeData linkedData) : this(contextGuid, contextType, new List<IEdgeData>(){linkedData})
        {
        }
        public AddEdgeCommand(string contextGuid, NovaElementType contextType, List<IEdgeData> linkedDataList) : base(contextGuid, contextType)
        {
            type = CommandType.Add_Edge;
            this.linkedDataList.AddRange(linkedDataList);
        }
        public override void onUndo()
        {
            foreach(var linkedData in linkedDataList)
            {
                linkedGraphView.removeGraphEdgeByCommand(linkedData);
            }
        }
        public override void onRedo() 
        {
            foreach (var linkedData in linkedDataList)
            {
                linkedGraphView.addGraphEdgeByCommand(linkedData);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddEdgeCommand addEdgeCommand) return;
            linkedDataList.AddRange(addEdgeCommand.linkedDataList);
        }
    }
}
