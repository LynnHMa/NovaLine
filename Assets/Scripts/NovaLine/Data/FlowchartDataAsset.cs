using NovaLine.Data.NodeGraphView;
using UnityEngine;

namespace NovaLine.File
{
    [CreateAssetMenu]
    public class FlowchartDataAsset : ScriptableObject
    {
        [SerializeReference]
        public FlowchartData data;

        public static FlowchartDataAsset CreateInstance(FlowchartData data = null)
        {
            var result = CreateInstance<FlowchartDataAsset>();
            result.data = data == null ? new FlowchartData() : data;
            return result;
        }
    }
}
