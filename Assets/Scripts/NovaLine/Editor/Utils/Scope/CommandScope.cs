using System;
using NovaLine.Editor.Window;

namespace NovaLine.Editor.Utils.Scope
{
    public class CommandScope : IDisposable
    {
        private readonly CommandRegistry handlingRegistry;
        public CommandScope()
        {
            handlingRegistry = CommandRegistry.Instance;
            if (handlingRegistry == null) return;
            handlingRegistry.beginRecordingCompoundCommand();
        }
        public void Dispose()
        {
            if (handlingRegistry == null) return;
            handlingRegistry.endRecordingCompoundCommand();
        }
    }
}
