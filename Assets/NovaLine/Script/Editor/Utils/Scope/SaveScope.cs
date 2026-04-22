using System;
using NovaLine.Script.Editor.File;
using UnityEditor;

namespace NovaLine.Script.Editor.Utils.Scope
{
    public class SaveScope : IDisposable
    {
        private static int scopeDepth;
        private static bool isDirty;
        
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
                EditorApplication.delayCall += TrySave;
            }
            else
            {
                TrySave();
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
                EditorFileManager.SaveCurrentGraphViewNodeData();       
            }
        }

        private static void TrySave()
        {
            scopeDepth--;
            if (scopeDepth == 0 && isDirty)
            {
                EditorFileManager.SaveCurrentGraphViewNodeData();
                isDirty = false;
            }
        }
    }
}
