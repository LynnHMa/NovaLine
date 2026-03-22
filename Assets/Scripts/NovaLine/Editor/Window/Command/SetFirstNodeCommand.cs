using System;
using Editor.Utils.Ext;
using NovaLine.Element;
using UnityEngine;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class SetFirstNodeCommand : Command
    {
        public KeyValue<string,string> firstNodeKeyValue;
        public SetFirstNodeCommand(string contextGuid, NovaElementType contextType, string oldFirstNodeGuid,string newFirstNodeGuid) : base(contextGuid, contextType)
        {
            type = CommandType.Set_First_Node;
            firstNodeKeyValue = new(oldFirstNodeGuid, newFirstNodeGuid);
        }

        public override void undo(bool autoSave = true)
        {
            if(linkedGraphView != null && firstNodeKeyValue.key != null)
            {
                linkedGraphView.setFirstNode(firstNodeKeyValue.key,false);
            }
            else
            {
                Debug.Log("failed to undo first node");
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = true)
        {
            if (linkedGraphView != null && firstNodeKeyValue.value != null)
            {
                linkedGraphView.setFirstNode(firstNodeKeyValue.value,false);
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not SetFirstNodeCommand setFirstNodeCommand) return;
            firstNodeKeyValue.value = setFirstNodeCommand.firstNodeKeyValue.value;
        }
    }
}
