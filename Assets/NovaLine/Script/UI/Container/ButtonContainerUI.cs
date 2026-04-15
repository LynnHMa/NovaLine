using System.Collections.Generic;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.UI.Container
{
    public class ButtonContainerUI : MonoBehaviour
    {
        public static ButtonContainerUI Instance { get; private set; }
        public Dictionary<string, NovaButton> Buttons { get; } = new();
        private void Awake()
        {
            Instance = this;
        }
        
        public static NovaButton GetButton(string name)
        {
            return Instance.Buttons[name];
        }

        public static NovaButton RegisterButton(string displayName, NovaButton buttonPrefab)
        {
            //todo calculate actual pos of button
            return null;
        }
        public static NovaButton RegisterButton(string displayName, NovaButton buttonPrefab,RectTransformChecker rectTransformChecker)
        {
            var instantiatedButton = Instantiate(buttonPrefab,Instance.gameObject.transform);
            if(instantiatedButton.text != null) instantiatedButton.text.text = displayName;
            Instance.Buttons.TryAdd(displayName, instantiatedButton);
            rectTransformChecker.LinkedTransform = instantiatedButton.transform;
            rectTransformChecker.ExportToTransform();
            return instantiatedButton;
        }

        public static void UnregisterButton(string name)
        {
            var toDestroyObj = Instance.Buttons[name]?.gameObject;
            if(toDestroyObj != null) Destroy(toDestroyObj);
            Instance.Buttons.Remove(name);
        }

        public static void ClearButtons()
        {
            foreach (var button in Instance.Buttons.Values)
            {
                if(button == null) continue;
                Destroy(button.gameObject);
            }
            Instance.Buttons.Clear();
        }
    }
}