using System;
using Editor.Utils.Ext;
using NovaLine.Element;
using UnityEngine;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public class InspectorElementChangeCommand : Command
    {
        public KeyValue<NovaElement,NovaElement> elementKeyValue;
        public InspectorElementChangeCommand(string contextGuid, NovaElementType contextType, NovaElement beforeChange,NovaElement afterChange) : base(contextGuid, contextType)
        {
            type = CommandType.Inspector_Change;
            elementKeyValue = new(beforeChange, afterChange);
        }

        public override void undo(bool autoSave = true)
        {
            if(elementKeyValue.key != null)
            {
                elementKeyValue.key.ReplaceToContext();
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = true)
        {
            if (elementKeyValue.value != null)
            {
                elementKeyValue.value.ReplaceToContext();
            }
            base.redo(autoSave);
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not InspectorElementChangeCommand inspectorElementChangeCommand) return;
            elementKeyValue.value = inspectorElementChangeCommand.elementKeyValue.value;
        }
    }
}
