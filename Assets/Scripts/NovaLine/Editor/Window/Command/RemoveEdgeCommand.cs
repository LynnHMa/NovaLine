using System.Collections.Generic;
using System;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Element;
using NovaLine.Element.Switcher;

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
        protected override void onUndo()
        {
            foreach (var linkedSwitcher in linkedSwitchers)
            {
                linkedGraphView.addGraphEdge(linkedGraphView.summonNewGraphEdge(linkedSwitcher),false);
            }
        }
        protected override void onRedo()
        {
            foreach (var linkedSwitcher in linkedSwitchers)
            {
                linkedGraphView.removeGraphEdge(linkedSwitcher.guid, false);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not RemoveEdgeCommand removeEdgeCommand) return;
            linkedSwitchers.AddRange(removeEdgeCommand.linkedSwitchers);
        }
    }
}
