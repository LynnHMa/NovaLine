using System;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.Save
{
    /// <summary>
    /// Provides extension points for overriding methods to implement custom logic (such as custom file saving and parameter processing) during save and load operations.
    /// </summary>
    [Serializable]
    public class ExampleSave : INovaSave
    {
        [SerializeField]
        private long _timestamp;
        [SerializeField]
        private string _nodeGUID;

        public long Timestamp => _timestamp;
        public string NodeGUID => _nodeGUID;
        
        public ExampleSave(string nodeGUID)
        {
            _nodeGUID = nodeGUID;
            _timestamp = TimeStampTool.GetNowTimeStamp();
        }
        public virtual void OnSave(){}
        public virtual void OnLoad() => NovaPlayer.PlayFromNode(NodeGUID);
    }

    public interface INovaSave
    {
        long Timestamp { get; }
        string NodeGUID { get; }
        void OnSave();
        void OnLoad();
    }
}