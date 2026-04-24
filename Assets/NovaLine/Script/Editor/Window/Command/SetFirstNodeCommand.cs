using System;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class SetFirstNodeCommand : Command
    {
        public KeyValue<string,string> firstNodeKeyValue;
        public SetFirstNodeCommand(string contextGUID, NovaElementType contextType, string oldFirstNodeGUID,string newFirstNodeGUID) : base(contextGUID, contextType)
        {
            Type = CommandType.Set_First_Node;
            firstNodeKeyValue = new(oldFirstNodeGUID, newFirstNodeGUID);
        }

        public override void OnUndo()
        {
            ParentGraphView.SetFirstNode(firstNodeKeyValue.key ?? "", false);
        }

        public override void OnRedo()
        {
            ParentGraphView.SetFirstNode(firstNodeKeyValue.value ?? "", false);
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not SetFirstNodeCommand setFirstNodeCommand) return;
            firstNodeKeyValue.value = setFirstNodeCommand.firstNodeKeyValue.value;
        }
    }
}
