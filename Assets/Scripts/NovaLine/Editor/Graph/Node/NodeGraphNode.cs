using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.Node
{
    using NovaLine.Editor.Graph.Port;
    using NovaLine.Editor.Graph.Edge;
    using UnityEditor.Experimental.GraphView;
    using NovaLine.Switcher;
    using UnityEngine.UIElements;
    using NovaLine.Editor.Graph.View;
    using NovaLine.Editor.Graph.Data;
    using NovaLine.Editor.Utils;

    [Serializable]
    public class NodeGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.red;
        public NodeGraphNode(Element.Node node, Vector2 pos) : base(node, pos)
        {
            addPort();
        }
        public NodeGraphNode(string title, Vector2 pos) : this(new Element.Node(title), pos)
        {
            addPort();
        }
        public override string getType()
        {
            return "[Node]";
        }

        protected override void onDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                var node = (Element.Node)linkedElement;

                var root = NovaGraphWindow.getMainWindowInstance()?.rootOpenedGraphView;
                var flowchartGraphViewData = (FlowchartGraphViewData)root.linkedData;
                if (flowchartGraphViewData == null)
                {
                    return;
                }

                NodeGraphViewData aimData = null;
                foreach (var nodeData in flowchartGraphViewData.nodeGraphViewDatas)
                {
                    if (nodeData.guid == node?.guid)
                    {
                        aimData = nodeData;
                    }
                }

                if (aimData == null)
                {
                    Debug.Log("Can't find stored data! Let us create new one!");
                    aimData = new NodeGraphViewData(node, pos);
                }
                evt.StopPropagation();

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    NovaGraphWindow.loadFlowchartInWindow(aimData, new NodeGraphView(node));
                };
            }
        }
        public override void addPort()
        {
            if (linkedElement is not Element.Node node) return;

            var input = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float), node, themedColor);

            input.portName = "In";

            var output = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float), node, themedColor);

            output.portName = "Out";

            inputContainer.Add(input);
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}