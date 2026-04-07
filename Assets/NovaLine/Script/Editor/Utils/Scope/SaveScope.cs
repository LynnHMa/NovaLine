using System;
using System.Collections.Generic;
using NovaLine.Script.Editor.File;
using NovaLine.Script.Editor.Window;
using UnityEditor;

namespace NovaLine.Script.Editor.Utils.Scope
{
    public class SaveScope : IDisposable
    {
        private static int scopeDepth = 0;
        private static bool isDirty = false;
        
        private readonly bool saveNextFrame;
        public SaveScope(bool saveNextFrame = false)
        {
            scopeDepth++;
            isDirty = true; 
            this.saveNextFrame = saveNextFrame;
        }
        
        public void Dispose()
        {
            if (saveNextFrame)
            {
                EditorApplication.delayCall += trySave;
            }
            else
            {
                trySave();
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

        private static void trySave()
        {
            scopeDepth--;
            if (scopeDepth == 0 && isDirty)
            {
                EditorFileManager.SaveGraphWindowData();
                isDirty = false;
            }
        }
    }
}
