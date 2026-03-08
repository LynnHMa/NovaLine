using System;

namespace NovaLine.Editor.Graph.Data
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Editor.Graph.Port;
    using NovaLine.Editor.Graph.View;
    using NovaLine.Switcher;
    using UnityEngine;

    [Serializable]
    public class GraphViewEdgeData<PE,EE,PVD,PVDE,N> : GraphViewData 
        where PE : NovaElement
        where EE : NovaSwitcher
        where PVD : GraphViewNodeData<PVDE> 
        where PVDE : NovaElement
        where N : GraphNode 
    {
        public EE linkedSwitcher { get; set; }
        public GraphViewEdgeData(EE linkedSwitcher)
        {
            this.linkedSwitcher = linkedSwitcher;
            guid = linkedSwitcher.guid;
        }

        public virtual void draw(PVD parentData ,INovaGraphView graphView) {
            if (graphView == null || graphView is not NovaGraphView<N, PE, EE> novaGraphView)
            {
                Debug.Log("Not in suitable graph view!");
                return;
            }

            var inputGraphNode = novaGraphView?.graphNodes?.Find(x => x.guid.Equals(linkedSwitcher?.inputElement?.guid));
            var outputGraphNode = novaGraphView?.graphNodes?.Find(x => x.guid.Equals(linkedSwitcher?.outputElement?.guid));

            if (inputGraphNode == null || outputGraphNode == null
                || inputGraphNode.inputContainer.childCount == 0 || inputGraphNode.outputContainer.childCount == 0
                || outputGraphNode.inputContainer.childCount == 0 || outputGraphNode.outputContainer.childCount == 0)
            {
                Debug.Log("cant find input or output node!");
                return;
            }

            var inputPort = inputGraphNode.inputContainer?[0] as GraphPort<PE, EE>;
            var outputPort = outputGraphNode.outputContainer?[0] as GraphPort<PE, EE>;

            if (inputPort == null || outputPort == null)
            {
                Debug.Log("cant find port!");
                return;
            }


            var edge = outputPort.ConnectTo<GraphEdge<PE,EE>>(inputPort, linkedSwitcher);
            edge.guid = guid;
            novaGraphView.AddElement(edge);
        }
    }
}
