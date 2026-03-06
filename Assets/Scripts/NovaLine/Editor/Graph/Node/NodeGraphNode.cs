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
    using NovaLine.Element;
    using NovaLine.Editor.Utils;

    [Serializable]
    public class NodeGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.red;
        public NodeGraphNode(Element.Node node, Vector2 pos) : base(node, pos)
        {
        }
        public NodeGraphNode(string title, Vector2 pos) : this(new Element.Node(title), pos)
        {
        }
        public override string getType()
        {
            return "[Node]";
        }

        protected override void onDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                var node = (Element.Node)targetObject;

                var root = NovaGraphWindow.getMainWindowInstance()?.rootOpenedGraphView;
                var flowchartGraphViewData = (FlowchartGraphViewData)root.linkedData;
                if (flowchartGraphViewData == null)
                {
                    return;
                }

                NodeGraphViewData aimData = null;
                foreach (var nodeData in flowchartGraphViewData.nodeGraphViewDatas)
                {
                    if (nodeData.guid == node?.guid) aimData = nodeData;
                }

                if (aimData == null)
                {
                    Debug.Log("cant find stored data!");
                    return;
                }
                evt.StopPropagation();

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (NovaGraphWindow.getMainWindowInstance() == null) return;

                    NovaGraphWindow.loadFlowchartInWindow(aimData, new NodeGraphView(node));
                };
            }
        }
        protected override void addPort(INovaElement element)
        {
            if (element is not Element.Node node) return;

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