using System;
using NovaLine.Script.Editor.Window;

namespace NovaLine.Script.Editor.Utils.Scope
{
    public class CommandScope : IDisposable
    {
        private readonly CommandRegistry handlingRegistry;
        public CommandScope()
        {
            handlingRegistry = CommandRegistry.Instance;
            if (handlingRegistry == null) return;
            handlingRegistry.BeginRecordingCompoundCommand();
        }
        public void Dispose()
        {
            if (handlingRegistry == null) return;
            handlingRegistry.EndRecordingCompoundCommand();
        }
    }
}
