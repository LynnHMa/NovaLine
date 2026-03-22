using Editor.Utils.Ext;
using NovaLine.Editor.File;
using NovaLine.Editor.Graph.View;
using NovaLine.Element;
using System;
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
        public virtual void undo(bool autoSave = true)
        {
            Debug.Log("[Undo] " + type);
            linkedGraphView?.update();
            if (autoSave) EditorFileManager.SaveGraphWindowData();
        }
        public virtual void redo(bool autoSave = true)
        {
            Debug.Log("[Redo] " + type);
            linkedGraphView?.update();
            if (autoSave) EditorFileManager.SaveGraphWindowData();
        }
        public abstract void merge(Command congenericCommand);
    }
    public interface ICommand
    {
        public CommandType type { get; set; }
        void undo(bool autoSave = true);
        void redo(bool autoSave = true);
        void merge(Command congenericCommand);
    }
    public enum CommandType
    {
        None, Inspector_Change, Add_Node, Remove_Node, Move_Node, Add_Edge, Remove_Edge, Set_First_Node, Compound
    }
}
