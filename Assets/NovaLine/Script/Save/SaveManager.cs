using System.Collections.Generic;

namespace NovaLine.Script.Save
{
    public static class SaveManager
    {
        //Replace with your custom save manager
        public static INovaSaveManager Manager { get; private set; } = new ExampleSaveManager();
    }
    
    /// <summary>
    /// Provides extension points for overriding methods to implement custom logic (such as custom file saving and parameter processing) during save and load operations.
    /// </summary>
    public class ExampleSaveManager : INovaSaveManager
    {
        /// <summary>
        /// Imported saves
        /// </summary>
        public List<INovaSave> Saves { get; } = new();

        /// <summary>
        /// Import saves by reading file JSON.
        /// </summary>
        public void ImportSave(){}

        /// <summary>
        /// Export saves to file JSON.
        /// </summary>
        public void ExportSave(){}

        /// <summary>
        /// Save in menu
        /// </summary>
        /// <param name="newSave">Save to add or override.</param>
        /// <param name="index">Index in Saves list</param>
        public int SaveInMenu(INovaSave newSave,int index = -1)
        {
            var actualIndex = index;
            if (index >= 0 && index < Saves.Count)
            {
                Saves[index] = newSave;
            }
            else
            {
                Saves.Add(newSave);
                actualIndex = Saves.Count - 1;
            }
            
            newSave?.OnSave();
            
            ExportSave();
            return actualIndex;
        }

        /// <summary>
        /// Load in menu
        /// </summary>
        /// <param name="index">Index in Saves list</param>
        public INovaSave LoadInMenu(int index = -1)
        {
            if (index < 0 || index >= Saves.Count) return null;
            
            var save = Saves[index];
            save?.OnLoad();
            return save;
        }

        /// <summary>
        /// Generate new save by custom parameters.
        /// </summary>
        /// <returns>New save</returns>
        public INovaSave CreateSave()
        {
            var nodeGUID = NovaPlayer.Instance.PlayingNodeGUID;
            return string.IsNullOrEmpty(nodeGUID) ? null : new ExampleSave(nodeGUID);
        }
    }

    public interface INovaSaveManager
    {
        List<INovaSave> Saves { get; }
        void ImportSave();
        void ExportSave();
        int SaveInMenu(INovaSave newSave,int index = -1);
        INovaSave LoadInMenu(int index = -1);
        INovaSave CreateSave();
    }
}