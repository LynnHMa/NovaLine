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
            LinkedElement = new Flowchart("New Flowchart");
        }

        public override void RegisterLinkedElement()
        {
            EntityRegistry.ClearEntities();
            if (Application.isPlaying)
            {
                for (var i = 0; i < LinkedElement.entityPrefabs.Count; i++)
                {
                    var entityPrefab = LinkedElement.entityPrefabs[i];
                    EntityRegistry.RegisterEntity(entityPrefab);
                }
            }
            base.RegisterLinkedElement();
        }

        public override void UnregisterLinkedElement()
        {
            EntityRegistry.ClearEntities();
            base.UnregisterLinkedElement();
        }
    }
}
