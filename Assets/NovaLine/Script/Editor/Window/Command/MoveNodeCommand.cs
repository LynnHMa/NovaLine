using UnityEngine;
using Editor.Utils.Ext;
using System.Collections.Generic;
using System;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class MoveNodeCommand : Command
    {
        public List<KeyValue<NovaElement,PosKeyValue>> movedGraphNodeInfo = new();
        public MoveNodeCommand(string contextGuid, NovaElementType contextType, KeyValue<NovaElement, PosKeyValue> situation) : this(contextGuid, contextType, new List<KeyValue<NovaElement, PosKeyValue>>() { situation })
        {
        }
        public MoveNodeCommand(string contextGuid, NovaElementType contextType, List<KeyValue<NovaElement, PosKeyValue>> situations) : base(contextGuid, contextType)
        {
            type = CommandType.Move_Node;
            movedGraphNodeInfo = situations;
        }

        public override void onUndo()
        {
            foreach(var graphNodeInfo in movedGraphNodeInfo)
            {
                var keyValue = graphNodeInfo.value;
                linkedGraphView.moveGraphNode(graphNodeInfo.key?.guid, keyValue.oldPos,false);
            }
        }
        public override void onRedo()
        {
            foreach (var graphNodeInfo in movedGraphNodeInfo)
            {
                var keyValue = graphNodeInfo.value;
                linkedGraphView.moveGraphNode(graphNodeInfo.key?.guid, keyValue.newPos,false);
            }
        }

        public override void merge(Command congenericCommand)
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
