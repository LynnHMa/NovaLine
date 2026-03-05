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
        public GraphViewEdgeData(EE switcher)
        {
            guid = switcher.guid;
        }

        public virtual void draw(PVD parentData ,INovaGraphView graphView) {
            if (graphView == null || graphView is not NovaGraphView<N, PE, EE> novaGraphView)
            {
                Debug.Log("unsupported");
                return;
            }
            var switcher = to(parentData);
            var input = novaGraphView?.graphNodes?.Find(x => x.guid.Equals(switcher?.outputElement?.guid))?.inputContainer?[0] as GraphPort<PE, EE>;
            var output = novaGraphView?.graphNodes?.Find(x => x.guid.Equals(switcher?.inputElement?.guid))?.outputContainer?[0] as GraphPort<PE, EE>;
            if (input == null || output == null) return; 
            var edge = output.ConnectTo<GraphEdge<PE,EE>>(input, switcher);
            edge.guid = guid;
            novaGraphView.AddElement(edge);
        }
        public virtual EE to(PVD parent) { return default; }
    }
}
