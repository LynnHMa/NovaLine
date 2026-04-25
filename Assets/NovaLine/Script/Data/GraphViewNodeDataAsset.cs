using System;
using NovaLine.Script.Data.NodeGraphView;
using UnityEngine;

namespace NovaLine.Script.Data
{
    public class GraphViewNodeDataAsset : ScriptableObject
    {
        [SerializeReference]
        public IGraphViewNodeData data;

        public static GraphViewNodeDataAsset CreateInstance(IGraphViewNodeData data)
        {
            try
            {
                if (data == null)
                {
                    throw new NullReferenceException("NovaAsset data is null");
                }
                var result = CreateInstance<GraphViewNodeDataAsset>();
                result.data = data;
                return result;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
