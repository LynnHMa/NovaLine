﻿﻿using System;
  using NovaLine.Script.Editor.Utils.Ext;
  using NovaLine.Script.Editor.Window;

namespace NovaLine.Script.Editor.Utils.Scope
{
    public class UpdateScope : IDisposable
    {
        private static int scopeDepth = 0;
        private static bool isDirty = false;
        private readonly int waitingFrameBeforeDispose;
        public UpdateScope(int waitingFrameBeforeDispose = 0)
        {
            scopeDepth++;
            isDirty = true;
            this.waitingFrameBeforeDispose = waitingFrameBeforeDispose;
        }
    
        public void Dispose()
        {
            if (waitingFrameBeforeDispose > 0)
            {
                CallbackExt.FrameCall(waitingFrameBeforeDispose,TryUpdateContext);
            }
            else
            {
                TryUpdateContext();
            }
        }
    
        public static void RequireUpdate()
        {
            if (scopeDepth > 0)
            {
                isDirty = true;
            }
            else
            {
                NovaWindow.UpdateContext();
            }
        }
    
        private static void TryUpdateContext()
        {
            scopeDepth--;
            if (scopeDepth == 0 && isDirty)
            {
                isDirty = false;
                NovaWindow.UpdateContext();
            }
        }
    }
}
