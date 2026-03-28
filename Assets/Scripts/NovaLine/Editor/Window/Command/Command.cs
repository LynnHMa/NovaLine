using Editor.Utils.Ext;
using NovaLine.Editor.Graph.View;
using NovaLine.Element;
using System;
using NovaLine.Editor.Utils.Scope;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Window.Command
{
    [Serializable]
    public abstract class Command : ICommand
    {
        public CommandType type = CommandType.None;
        public KeyValue<string, NovaElementType> linkedContextInfo;

        protected INovaGraphView linkedGraphView => linkedContextInfo == null ? null : GetContext(linkedContextInfo.key,linkedContextInfo.value)?.graphView;

        protected Command() { }
        public Command(string contextGuid,NovaElementType contextType)
        {
            linkedContextInfo = new(contextGuid, contextType);
        }

        CommandType ICommand.type { get => type; set => type = value; }

        public void undo()
        {
            if (linkedGraphView == null) return;
            
            //Debug.Log("[Undo] " + type);
            using (new UpdateScope())
            using (new SaveScope())
            {
                onUndo();
            }
            linkedGraphView?.update();
        }

        public void redo()
        {
            if (linkedGraphView == null) return;
            
            //Debug.Log("[Redo] " + type);
            using (new UpdateScope())
            using (new SaveScope())
            {
                onRedo();
            }
            linkedGraphView?.update();
        }

        protected abstract void onUndo();

        protected abstract void onRedo();
        public abstract void merge(Command congenericCommand);
    }
    public interface ICommand
    {
        public CommandType type { get; set; }
        void undo();
        void redo();
        void merge(Command congenericCommand);
    }
    public enum CommandType
    {
        None, Inspector_Change, Add_Node, Remove_Node, Move_Node, Add_Edge, Remove_Edge, Set_First_Node, Compound
    }
}
