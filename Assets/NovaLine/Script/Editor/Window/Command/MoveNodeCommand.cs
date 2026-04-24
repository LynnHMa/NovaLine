using UnityEngine;
using System.Collections.Generic;
using System;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class MoveNodeCommand : Command
    {
        public List<KeyValue<NovaElement,PosKeyValue>> movedGraphNodeInfo = new();
        public MoveNodeCommand(string contextGUID, NovaElementType contextType, KeyValue<NovaElement, PosKeyValue> situation) : this(contextGUID, contextType, new List<KeyValue<NovaElement, PosKeyValue>>() { situation })
        {
        }
        public MoveNodeCommand(string contextGUID, NovaElementType contextType, List<KeyValue<NovaElement, PosKeyValue>> situations) : base(contextGUID, contextType)
        {
            Type = CommandType.Move_Node;
            movedGraphNodeInfo = situations;
        }

        public override void OnUndo()
        {
            foreach(var graphNodeInfo in movedGraphNodeInfo)
            {
                var keyValue = graphNodeInfo.value;
                ParentGraphView.MoveGraphNode(graphNodeInfo.key?.GUID, keyValue.oldPos,false);
            }
        }
        public override void OnRedo()
        {
            foreach (var graphNodeInfo in movedGraphNodeInfo)
            {
                var keyValue = graphNodeInfo.value;
                ParentGraphView.MoveGraphNode(graphNodeInfo.key?.GUID, keyValue.newPos,false);
            }
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not MoveNodeCommand moveNodeCommand) return;
            movedGraphNodeInfo.AddRange(moveNodeCommand.movedGraphNodeInfo);
        }
    }
    [Serializable]
    public class PosKeyValue : KeyValue<Vector2, Vector2>
    {
        public Vector2 oldPos => key;
        public Vector2 newPos => value;
        public PosKeyValue(Vector2 key, Vector2 value) : base(key, value) { }
    }
}
