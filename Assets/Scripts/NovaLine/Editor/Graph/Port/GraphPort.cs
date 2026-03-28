using System;
using System.Collections.Generic;
using NovaLine.Element.Switcher;
using UnityEngine.UIElements;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Graph.Port
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Edge;
    using UnityEditor.Experimental.GraphView;
    using System.Linq;

    public class GraphPort<PE,EE> : Port where EE : NovaSwitcher where PE : NovaElement
    {
        public PE ownerElement { get; set; }
        public GraphPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type,PE ownerElement) : base(portOrientation, portDirection, portCapacity, type)
        {
            this.ownerElement = ownerElement;
        }
        public static GraphPort<PE,EE> Create<ED>(Orientation orientation, Direction direction, Capacity capacity, Type type, PE ownerElement,Color color,string portName) where ED : GraphEdge<PE,EE>, new()
        {
            CustomEdgeConnectorListener<PE,EE,ED> listener = new();
            var port = new GraphPort<PE,EE>(orientation, direction, capacity, type, ownerElement)
            {
                m_EdgeConnector = new EdgeConnector<ED>(listener)
            };
            port.portColor = Color.white;
            port.portName = portName;

            var inputPortLabel = port.Q<Label>();
            if (inputPortLabel != null)
            {
                inputPortLabel.style.fontSize = 13;
                inputPortLabel.style.color = Color.white;
                inputPortLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            port.ownerElement = ownerElement;
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        public T ConnectTo<T>(GraphPort<PE, EE> other, EE edgeElement) where T : GraphEdge<PE,EE>, new()
        {
            if (other == null)
            {
                throw new ArgumentNullException("Port.ConnectTo<T>() other argument is null");
            }

            if (other.direction == direction)
            {
                throw new ArgumentException("Cannot connect two ports with the same direction");
            }

            T val = new T
            {
                output = ((direction == Direction.Output) ? this : other),
                input = ((direction == Direction.Input) ? this : other)
            };

            val.linkedElement = edgeElement;
            val.input = other;
            val.output = this;
            Connect(val,false);
            other.Connect(val);
            return val;
        }

        public override void Connect(Edge edge)
        {
            Connect(edge, true);
        }
        public void Connect(Edge edge,bool registerCommand)
        {
            base.Connect(edge);
            if (edge is GraphEdge<PE, EE> graphEdge)
            {
                if (graphEdge.linkedElement == null) graphEdge.generateNewLinkedElement();
                if (graphEdge.linkedElement == null || graphEdge.input.ownerElement.guid == ownerElement.guid) return;
                graphEdge.linkedElement.outputElement = ownerElement;
                graphEdge.linkedElement.inputElement = graphEdge.input.ownerElement;

                ownerElement.onGraphConnect(graphEdge.linkedElement);

                CurrentGraphViewContext.graphView.addGraphEdge(graphEdge, registerCommand);
            }
        }
        public override void Disconnect(Edge edge)
        {
            Disconnect(edge, true);
        }
        public void Disconnect(Edge edge,bool registerCommand)
        {
            if (!connections.Contains(edge)) return;
            base.Disconnect(edge);
            if (edge is GraphEdge<PE, EE> graphEdge)
            {
                if (graphEdge.input.ownerElement.guid == ownerElement.guid) return;
                ownerElement.onGraphDisconnect(graphEdge.linkedElement);

                CurrentGraphViewContext.graphView.removeGraphEdge(graphEdge, registerCommand);

                graphEdge.RemoveFromHierarchy();
            }
        }
    }
    public class CustomEdgeConnectorListener<PE,EE,ED> : IEdgeConnectorListener where ED : GraphEdge<PE, EE> where EE : NovaSwitcher where PE : NovaElement
    {
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate;
        private List<GraphElement> m_EdgesToDelete;

        public CustomEdgeConnectorListener()
        {
            m_EdgesToCreate = new();
            m_EdgesToDelete = new();
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();

            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (var connection in edge.input.connections)
                    if (connection != edge) m_EdgesToDelete.Add(connection);
            }

            if (edge.output.capacity == Port.Capacity.Single)
            {
                foreach (var connection in edge.output.connections)
                    if (connection != edge) m_EdgesToDelete.Add(connection);
            }

            if (m_EdgesToDelete.Count > 0)
                graphView.DeleteElements(m_EdgesToDelete);

            var edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (var e in edgesToCreate)
            {
                graphView.AddElement(e);
                if(e is ED ed && edge is ED ed2)
                {
                    ed2.input.Connect(ed);
                    ed2.output.Connect(ed);
                }
            }
        }
    }
}
