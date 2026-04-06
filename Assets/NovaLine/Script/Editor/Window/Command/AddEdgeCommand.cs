using System.Collections.Generic;
using System;
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
        public List<string> linkedSwitcherGuidList = new();

        public AddEdgeCommand(string contextGuid, NovaElementType contextType, IGraphEdge graphEdge) : this(contextGuid, contextType, new List<IGraphEdge>(){graphEdge})
        {
        }
        public AddEdgeCommand(string contextGuid, NovaElementType contextType, List<IGraphEdge> graphEdges) : base(contextGuid, contextType)
        {
            type = CommandType.Add_Edge;
            foreach (var graphEdge in graphEdges)
            {
                linkedSwitcherGuidList.Add(graphEdge.guid);
            }
        }
        public override void onUndo()
        {
            foreach(var guid in linkedSwitcherGuidList)
            {
                linkedGraphView.removeGraphEdge(guid,false);
            }
        }
        public override void onRedo() 
        {
            foreach (var guid in linkedSwitcherGuidList)
            {
                var linkedSwitcher = FindElement(guid) as NovaSwitcher;
                linkedGraphView.addGraphEdge(linkedGraphView.summonNewGraphEdge(linkedSwitcher),false);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not AddEdgeCommand addEdgeCommand) return;
            linkedSwitcherGuidList.AddRange(addEdgeCommand.linkedSwitcherGuidList);
        }
    }
}
