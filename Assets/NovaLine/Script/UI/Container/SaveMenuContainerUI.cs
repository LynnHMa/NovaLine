using System.Collections.Generic;
using NovaLine.Script.Save;
using TMPro;
using UnityEngine;

namespace NovaLine.Script.UI.Container
{
    public enum SaveMenuMode
    {
        Save,
        Load
    }
    public class SaveMenuContainerUI : NovaContainerUI
    {
        public static SaveMenuContainerUI Instance { get; private set; }
        private List<SaveButton> SaveButtons { get; } = new();
        
        public TextMeshProUGUI topTitleText;
        public RectTransform saveViewportContentTransform;
        public SaveButton saveButtonPrefab;
        public float buttonSpace = 20;
        
        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }
        
        public void OpenSaveMenu()
        {
            Open();
            AddSaveButtonsInMenu(SaveMenuMode.Save);
            topTitleText.text = "Save";
        }

        public void OpenLoadMenu()
        {
            Open();
            AddSaveButtonsInMenu(SaveMenuMode.Load);
            topTitleText.text = "Load";
        }
        
        public void CloseSaveMenu()
        {
            Close();
            ClearSaveButtonsInMenu();
        }

        public override void Close()
        {
            base.Close();
            MainMenuContainerUI.Instance.OpenMainMenu();
        }

        public override void Open()
        {
            base.Open();
            MainMenuContainerUI.Instance.CloseMainMenu();
        }

        public void UpdateContent(SaveMenuMode mode)
        {
            ClearSaveButtonsInMenu();
            AddSaveButtonsInMenu(mode);
        }
        
        private void AddSaveButtonsInMenu(SaveMenuMode menuMode)
        {
            if (saveButtonPrefab == null)
            {
                Debug.LogError("Can't find save button prefab!");
                return;
            }
            
            SaveManager.Manager.ImportSave();
            var saves = SaveManager.Manager.Saves;
            if(saves.Count == 0 && menuMode == SaveMenuMode.Load) return;

            var buttonHeight = saveButtonPrefab.GetComponent<RectTransform>().rect.height;
            var contentHeight = Mathf.Max(700,100 + (buttonHeight + buttonSpace) * saves.Count);
            saveViewportContentTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,contentHeight);
            var additionalButton = menuMode == SaveMenuMode.Save ? 1 : 0;
            for (var i = 0; i < saves.Count + additionalButton; i++)
            {
                var actualRectPos = new Vector2(0,-50 - i * (buttonHeight + buttonSpace));
                
                var button = Instantiate(saveButtonPrefab,actualRectPos,Quaternion.identity);
                button.RebindSave(i);
                button.ChangeMode(menuMode);
                if (saveViewportContentTransform != null)
                {
                    button.transform.SetParent(saveViewportContentTransform,false);
                }

                if (i == saves.Count)
                {
                    button.TextMeshProUGUI.text = "+ New Save";
                }
                
                SaveButtons.Add(button);
            }
        }

        private void ClearSaveButtonsInMenu()
        {
            if (SaveButtons.Count == 0) return;
            for (var i = SaveButtons.Count - 1; i >= 0; i--)
            {
                var button = SaveButtons[i];
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            SaveButtons.Clear();
        }
    }
}