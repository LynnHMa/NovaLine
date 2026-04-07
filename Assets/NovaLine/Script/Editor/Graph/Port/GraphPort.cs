﻿using System;
using System.Collections.Generic;
using NovaLine.Script.Element.Switcher;
using UnityEngine.UIElements;
using UnityEngine;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Graph.Port
{
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Edge;
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
                m_EdgeConnector = new EdgeConnector<ED>(listener),
                portColor = Color.white,
                portName = portName
            };

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

        //Create an edge and connect two port (Not connect by hand)
        public T ConnectTo<T>(GraphPort<PE, EE> targetPort, EE linkedSwitcher) where T : GraphEdge<PE,EE>, new()
        {
            if (targetPort == null)
            {
                throw new ArgumentNullException("Port.ConnectTo<T>() other argument is null");
            }

            if (targetPort.direction == direction)
            {
                throw new ArgumentException("Cannot connect two ports with the same direction");
            }

            T graphEdge = new T
            {
                output = ((direction == Direction.Output) ? this : targetPort),
                input = ((direction == Direction.Input) ? this : targetPort),
                linkedElement = linkedSwitcher
            };

            graphEdge.input = targetPort;
            graphEdge.output = this;
            connect(graphEdge,false);
            targetPort.connect(graphEdge,false);
            return graphEdge;
        }

        //Connect by hand
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            connect(edge, true);
        }
        public void connect(Edge edge,bool isByHand)
        {
            if (edge is GraphEdge<PE, EE> graphEdge)
            {
                if (graphEdge.linkedElement == null) graphEdge.generateNewLinkedElement();
                if (graphEdge.linkedElement == null || graphEdge.input.ownerElement.guid == ownerElement.guid) return;
                graphEdge.linkedElement.outputElementGuid = ownerElement.guid;
                graphEdge.linkedElement.inputElementGuid = graphEdge.input.ownerElement.guid;

                ownerElement.onGraphConnect(graphEdge.linkedElement);

                if (isByHand)
                {
                    CurrentGraphViewContext.graphView.addGraphEdgeByHand(graphEdge);
                }
                else
                {
                    CurrentGraphViewContext.graphView.addGraphEdge(graphEdge);
                }

            }
        }
        
        //Disconnect by hand
        public override void Disconnect(Edge edge)
        {
            disconnect(edge, true);
        }
        public void disconnect(Edge edge,bool isByHand)
        {
            if (!connections.Contains(edge)) return;
            base.Disconnect(edge);
            if (edge is GraphEdge<PE, EE> graphEdge)
            {
                if (graphEdge.input.ownerElement.guid == ownerElement.guid) return;
                ownerElement.onGraphDisconnect(graphEdge.linkedElement);

                if (isByHand)
                {
                    CurrentGraphViewContext.graphView.removeGraphEdgeByHand(graphEdge);
                }
                else
                {
                    CurrentGraphViewContext.graphView.removeGraphEdge(graphEdge);
                }

                graphEdge.RemoveFromHierarchy();
            }
        }
    }
    public class CustomEdgeConnectorListener<PE,EE,ED> : IEdgeConnectorListener where ED : GraphEdge<PE, EE> where EE : NovaSwitcher where PE : NovaElement
    {
        private readonly GraphViewChange m_GraphViewChange;
        private readonly List<Edge> m_EdgesToCreate;
        private readonly List<GraphElement> m_EdgesToDelete;

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
