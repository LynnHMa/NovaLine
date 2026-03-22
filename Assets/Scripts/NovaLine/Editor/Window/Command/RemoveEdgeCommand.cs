using System.Collections.Generic;
using System;
using NovaLine.Switcher;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class RemoveEdgeCommand : Command
    {
        public List<NovaSwitcher> linkedSwitchers = new();

        public RemoveEdgeCommand(string contextGuid, NovaElementType contextType, IGraphEdge graphEdge) : this(contextGuid, contextType, new List<IGraphEdge>() { graphEdge })
        {
        }
        public RemoveEdgeCommand(string contextGuid, NovaElementType contextType, List<IGraphEdge> graphEdges) : base(contextGuid, contextType)
        {
            type = CommandType.Remove_Edge;
            foreach (var graphEdge in graphEdges)
            {
                linkedSwitchers.Add(graphEdge.linkedElement);
            }
        }
        public override void undo(bool autoSave = true)
        {
            if (linkedGraphView != null)
            {
                foreach (var linkedSwitcher in linkedSwitchers)
                {
                    linkedGraphView.addGraphEdge(linkedGraphView.summonNewGraphEdge(linkedSwitcher), false, false,false);
                }
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = true)
        {
            if (linkedGraphView != null)
            {
                foreach (var linkedSwitcher in linkedSwitchers)
                {
                    linkedGraphView.removeGraphEdge(linkedSwitcher.guid, false, false);
                }
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveEdgeCommand removeEdgeCommand) return;
            linkedSwitchers.AddRange(removeEdgeCommand.linkedSwitchers);
        }
    }
}
