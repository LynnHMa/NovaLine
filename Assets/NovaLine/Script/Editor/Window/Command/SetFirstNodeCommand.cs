using System;
using Editor.Utils.Ext;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
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

        public override void onUndo()
        {
            if(firstNodeKeyValue.key != null)
            {
                linkedGraphView.setFirstNode(firstNodeKeyValue.key,false);
            }
        }
        public override void onRedo()
        {
            if (firstNodeKeyValue.value != null)
            {
                linkedGraphView.setFirstNode(firstNodeKeyValue.value,false);
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not SetFirstNodeCommand setFirstNodeCommand) return;
            firstNodeKeyValue.value = setFirstNodeCommand.firstNodeKeyValue.value;
        }
    }
}
