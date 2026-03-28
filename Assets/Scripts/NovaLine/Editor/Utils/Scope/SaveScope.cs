using System;
using System.Collections.Generic;
using NovaLine.Editor.File;
using NovaLine.Editor.Window;

namespace NovaLine.Editor.Utils.Scope
{
    public class SaveScope : IDisposable
    {
        private static int scopeDepth = 0;
        private static bool isDirty = false;
        
        public SaveScope()
        {
            scopeDepth++;
            isDirty = true; 
        }
        
        public void Dispose()
        {
            scopeDepth--;
            if (scopeDepth == 0 && isDirty)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }

        public static void RequireSave()
        {
            if (scopeDepth > 0)
            {
                isDirty = true;
            }
            else
            {
                EditorFileManager.SaveGraphWindowData();       
            }
        }
    }
}
