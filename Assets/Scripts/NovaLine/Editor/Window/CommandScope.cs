using System;
using System.Collections.Generic;

namespace NovaLine.Editor.Window
{
    public class CommandScope : IDisposable
    {
        private List<Command.Command> scopeCache = new();
        private CommandRegistry hanldingRegistry;
        public CommandScope()
        {
            hanldingRegistry = CommandRegistry.Instance;
            if (hanldingRegistry == null) return;
            hanldingRegistry.beginRecordingCompoundCommand();
        }
        public void Dispose()
        {
            if (hanldingRegistry == null) return;
            hanldingRegistry.endRecordingCompoundCommand();
        }
    }
}
