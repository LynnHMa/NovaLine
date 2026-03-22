using System.Collections.Generic;
using System;
using NovaLine.Switcher;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class AddEdgeCommand : Command
    {
        public List<NovaSwitcher> linkedSwitchers = new();

        public AddEdgeCommand(string contextGuid, NovaElementType contextType, IGraphEdge graphEdge) : this(contextGuid, contextType, new List<IGraphEdge>(){graphEdge})
        {
        }
        public AddEdgeCommand(string contextGuid, NovaElementType contextType, List<IGraphEdge> graphEdges) : base(contextGuid, contextType)
        {
            type = CommandType.Add_Edge;
            foreach (var graphEdge in graphEdges)
            {
                linkedSwitchers.Add(graphEdge.linkedElement);
            }
        }
        public override void undo(bool autoSave = false)
        {
            if(linkedGraphView != null)
            {
                foreach(var linkedSwitcher in linkedSwitchers)
                {
                    linkedGraphView.removeGraphEdge(linkedSwitcher.guid, false, false);
                }
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = false)
        {
            if (linkedGraphView != null)
            {
                foreach (var linkedSwitcher in linkedSwitchers)
                {
                    linkedGraphView.addGraphEdge(linkedGraphView.summonNewGraphEdge(linkedSwitcher), false, false,false);
                }
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddEdgeCommand addEdgeCommand) return;
            linkedSwitchers.AddRange(addEdgeCommand.linkedSwitchers);
        }
    }
}
