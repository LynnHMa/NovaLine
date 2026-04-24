using System;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Data.NodeGraphView
{
    using NovaLine.Script.Element;
    using UnityEngine;
    using NovaLine.Script.Data.Edge;

    [Serializable]
    public class NodeData : GraphViewNodeData<Node, ActionData, ActionEdgeData>,IAroundConditionData
    {
        [SerializeReference,HideInInspector] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference,HideInInspector] private ConditionData _conditionAfterInvokeData;
        public ConditionData ConditionBeforeInvokeData
        {
            get => _conditionBeforeInvokeData;
            set => _conditionBeforeInvokeData = value;
        }
        public ConditionData ConditionAfterInvokeData
        {
            get => _conditionAfterInvokeData;
            set => _conditionAfterInvokeData = value;
        }

        public NodeData(){}
        public NodeData(Node node,Vector2 pos)
        {
            Pos = pos;
            LinkedElement = node;
            ConditionBeforeInvokeData = new ConditionData(LinkedElement?.ConditionBeforeInvoke);
            ConditionAfterInvokeData = new ConditionData(LinkedElement?.ConditionAfterInvoke);
        }

        public override INovaData Copy()
        {
            var nodeData = (NodeData)base.Copy();
            
            if (nodeData == null) return null;

            if (ConditionBeforeInvokeData != null && ConditionAfterInvokeData != null)
            {
                nodeData.ConditionBeforeInvokeData = (ConditionData)ConditionBeforeInvokeData.Copy();
                nodeData.ConditionAfterInvokeData  = (ConditionData)ConditionAfterInvokeData.Copy();
                if (nodeData.LinkedElement is IAroundConditionElement condElement)
                {
                    condElement.ConditionBeforeInvokeGUID = nodeData.ConditionBeforeInvokeData.LinkedElement?.GUID;
                    condElement.ConditionAfterInvokeGUID  = nodeData.ConditionAfterInvokeData.LinkedElement?.GUID;
                    nodeData.ConditionBeforeInvokeData.LinkedElement?.SetParent(nodeData.LinkedElement); 
                    nodeData.ConditionAfterInvokeData.LinkedElement?.SetParent(nodeData.LinkedElement); 
                }
            }

            return nodeData;
        }
        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                ConditionBeforeInvokeData?.UpdateLinkedElement();
                ConditionAfterInvokeData?.UpdateLinkedElement();
            }
            base.UpdateLinkedElement(updateChildren);
        }
        public override void RegisterLinkedElement()
        {
            ConditionBeforeInvokeData?.LinkedElement?.SetParent(LinkedElement);
            ConditionAfterInvokeData?.LinkedElement?.SetParent(LinkedElement);
            ConditionBeforeInvokeData?.RegisterLinkedElement();
            ConditionAfterInvokeData?.RegisterLinkedElement();
            base.RegisterLinkedElement();
        }

        public override void UnregisterLinkedElement()
        {
            ConditionBeforeInvokeData?.UnregisterLinkedElement();
            ConditionAfterInvokeData?.UnregisterLinkedElement();
            base.UnregisterLinkedElement();
        }
    }
}
