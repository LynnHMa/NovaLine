using System.Collections.Generic;
using System;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Utils.Scope;
using NovaLine.Element;
using NovaLine.Element.Switcher;

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
        protected override void onUndo()
        {
            foreach(var linkedSwitcher in linkedSwitchers)
            {
                linkedGraphView.removeGraphEdge(linkedSwitcher.guid,false);
            }
        }
        protected override void onRedo() 
        {
            foreach (var linkedSwitcher in linkedSwitchers)
            {
                linkedGraphView.addGraphEdge(linkedGraphView.summonNewGraphEdge(linkedSwitcher),false);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddEdgeCommand addEdgeCommand) return;
            linkedSwitchers.AddRange(addEdgeCommand.linkedSwitchers);
        }
    }
}
