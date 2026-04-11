using NovaLine.Script.Data.Edge;
using NovaLine.Script.Element;
using System;
using UnityEngine;

namespace NovaLine.Script.Data.NodeGraphView
{
    [Serializable]
    public class FlowchartData : GraphViewNodeData<Flowchart, NodeData, NodeEdgeData>
    {
        public FlowchartData()
        {
            linkedElement = new Flowchart("New Flowchart");
        }

        public override void registerLinkedElement()
        {
            EntityRegistry.ClearEntities();
            NovaElementRegistry.Clear();
            if (Application.isPlaying)
            {
                for (var i = 0; i < linkedElement.entityPrefabs.Count; i++)
                {
                    var entityPrefab = linkedElement.entityPrefabs[i];
                    EntityRegistry.RegisterEntity(entityPrefab);
                }
            }
            base.registerLinkedElement();
        }
    }
}
