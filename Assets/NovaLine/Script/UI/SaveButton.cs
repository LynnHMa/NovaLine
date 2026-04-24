using NovaLine.Script.Save;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI
{
    public class SaveButton : MonoBehaviour
    {
        private static INovaSaveManager SM => SaveManager.Manager;
        public Button Button { get; private set; }
        public TextMeshProUGUI TextMeshProUGUI { get; private set; }
        public int Index { get; set; }
        
        private void Awake()
        {
            Button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            TextMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ChangeMode(SaveMenuMode mode)
        {
            void OnSaveButtonClick(SaveMenuMode saveMenuMode)
            {
                switch (saveMenuMode)
                {
                    case SaveMenuMode.Save:
                        var newSave = SM.CreateSave();
                        SM.SaveInMenu(newSave,Index);
                        SaveMenuContainerUI.Instance.UpdateContent(mode);
                        break;
                    case SaveMenuMode.Load:
                        SM.LoadInMenu(Index);
                        break;
                }
            }
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() => OnSaveButtonClick(mode));
        }
        public void RebindSave(int index)
        {
            Index = index;
            
            if (Index < 0 || Index >= SM.Saves.Count) return;
            
            var bindingSave = SM.Saves[Index];
            
            if (bindingSave != null && TextMeshProUGUI != null)
            {
                TextMeshProUGUI.text = TimeStampTool.ToDateTimeString(bindingSave.Timestamp);
            }
        }
    }
}
