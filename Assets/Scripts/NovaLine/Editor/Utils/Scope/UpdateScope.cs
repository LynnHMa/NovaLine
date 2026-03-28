using System;
using System.Collections.Generic;
using NovaLine.Editor.File;
using NovaLine.Editor.Graph.View;
using NovaLine.Editor.Window;

namespace NovaLine.Editor.Utils.Scope
{
    public class UpdateScope : IDisposable
    {
        private static int scopeDepth = 0;
        private static bool isDirty = false;
        
        public UpdateScope()
        {
            scopeDepth++;
            isDirty = true; 
        }
        
        public void Dispose()
        {
            scopeDepth--;
            if (scopeDepth == 0 && isDirty)
            {
                NovaWindow.UpdateContext();
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
    }
}
