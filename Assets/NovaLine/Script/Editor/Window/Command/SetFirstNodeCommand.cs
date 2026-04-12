using System;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class SetFirstNodeCommand : Command
    {
        public KeyValue<string,string> firstNodeKeyValue;
        public SetFirstNodeCommand(string contextGuid, NovaElementType contextType, string oldFirstNodeGuid,string newFirstNodeGuid) : base(contextGuid, contextType)
        {
            Type = CommandType.Set_First_Node;
            firstNodeKeyValue = new(oldFirstNodeGuid, newFirstNodeGuid);
        }

        public override void OnUndo()
        {
            linkedGraphView.SetFirstNode(firstNodeKeyValue.key ?? "", false);
        }

        public override void OnRedo()
        {
            linkedGraphView.SetFirstNode(firstNodeKeyValue.value ?? "", false);
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not SetFirstNodeCommand setFirstNodeCommand) return;
            firstNodeKeyValue.value = setFirstNodeCommand.firstNodeKeyValue.value;
        }
    }
}
