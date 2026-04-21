using System;
using NovaLine.Script.Data;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class InspectorElementChangeCommand : Command
    {
        public INovaData beforeData;
        public INovaData afterData;

        public InspectorElementChangeCommand(
            string contextGuid, NovaElementType contextType,
            INovaData beforeData,INovaData afterData)
            : base(contextGuid, contextType)
        {
            Type = CommandType.Inspector_Change;
            this.beforeData = beforeData;
            this.afterData = afterData;
        }

        public override void OnUndo()
        {
            InspectorHelper.GlobalReplace(afterData, beforeData,false);
            beforeData.LinkedElement.ShowInInspector();
        }

        public override void OnRedo()
        {
            InspectorHelper.GlobalReplace(beforeData, afterData,false);
            afterData.LinkedElement.ShowInInspector();
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not InspectorElementChangeCommand other) return;
        
            if (beforeData != other.beforeData || afterData != other.afterData) return;

            afterData = other.afterData;
        }
    }
}