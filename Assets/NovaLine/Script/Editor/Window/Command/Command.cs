using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Element;
using System;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public abstract class Command : ICommand
    {
        public KeyValue<string, NovaElementType> linkedContextInfo;

        public CommandType Type = CommandType.None;
        protected INovaGraphView linkedGraphView => linkedContextInfo == null ? null : (GetContext(linkedContextInfo.key,linkedContextInfo.value) as IGraphViewNodeContext)?.GraphView;

        protected Command() { }
        protected Command(string contextGuid,NovaElementType contextType)
        {
            linkedContextInfo = new(contextGuid, contextType);
        }

        CommandType ICommand.Type { get => Type; set => Type = value; }

        public void Undo()
        {
            if (linkedGraphView == null) return;
            
            //Debug.Log("[Undo] " + type);
            using (new UpdateScope())
            using (new SaveScope())
            {
                OnUndo();
            }
            linkedGraphView?.Update();
        }

        public void Redo()
        {
            if (linkedGraphView == null) return;
            
            //Debug.Log("[Redo] " + type);
            using (new UpdateScope())
            using (new SaveScope())
            {
                OnRedo();
            }
            linkedGraphView?.Update();
        }

        public abstract void OnUndo();

        public abstract void OnRedo();
        public abstract void Merge(Command congenericCommand);
    }
    public interface ICommand
    {
        public CommandType Type { get; set; }
        void Undo();
        void Redo();
        void Merge(Command congenericCommand);
    }
    public enum CommandType
    {
        None, Inspector_Change, Add_Node, Remove_Node, Move_Node, Add_Edge, Remove_Edge, Set_First_Node, Compound
    }
}
