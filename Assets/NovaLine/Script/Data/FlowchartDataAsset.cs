using NovaLine.Script.Data.NodeGraphView;
using UnityEngine;

namespace NovaLine.Script.Data
{
    [CreateAssetMenu]
    public class FlowchartDataAsset : ScriptableObject
    {
        [SerializeReference,HideInInspector]
        public FlowchartData data;

        public static FlowchartDataAsset CreateInstance(FlowchartData data = null)
        {
            var result = CreateInstance<FlowchartDataAsset>();
            result.data = data == null ? new FlowchartData() : data;
            return result;
        }
    }
}
