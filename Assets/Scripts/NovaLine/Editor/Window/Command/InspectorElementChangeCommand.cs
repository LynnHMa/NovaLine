using System;
using Editor.Utils.Ext;
using NovaLine.Element;

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

        protected override void onUndo()
        {
            if(elementKeyValue.key != null)
            {
                elementKeyValue.key.ReplaceToContext();
            }
        }
        protected override void onRedo()
        {
            if (elementKeyValue.value != null)
            {
                elementKeyValue.value.ReplaceToContext();
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not InspectorElementChangeCommand inspectorElementChangeCommand) return;
            elementKeyValue.value = inspectorElementChangeCommand.elementKeyValue.value;
        }
    }
}
