using System.Collections.Generic;
using NovaLine.Script.UI;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.Registry
{
    public static class ButtonRegistry
    {
        private static ButtonContainerUI ContainerUI => ButtonContainerUI.Instance;
        public static Dictionary<string, OptionButton> Buttons { get; } = new();

        public static OptionButton GetButton(string name)
        {
            return Buttons.GetValueOrDefault(name);
        }

        public static OptionButton RegisterButton(string displayName, OptionButton buttonPrefab, RectTransformChecker rectTransformChecker = null)
        {
            var instantiatedButton = Object.Instantiate(buttonPrefab, ContainerUI.transform, false);
            
            if (instantiatedButton.text != null) instantiatedButton.text.text = displayName;
            Buttons.TryAdd(displayName, instantiatedButton);
            
            instantiatedButton.IsDefaultRectTransform = rectTransformChecker == null;
            
            if (rectTransformChecker != null)
            {
                rectTransformChecker.LinkedTransform = instantiatedButton.transform;
                rectTransformChecker.ExportToTransform();
            }
            else
            {
                ContainerUI.RefreshDefaultRectTransformButtons();
            }
            return instantiatedButton;
        }

        public static void UnregisterButton(string name)
        {
            if (!Buttons.TryGetValue(name, out var toDestroyButton)) return;
            
            if (toDestroyButton != null && toDestroyButton.gameObject != null) 
            {
                Object.Destroy(toDestroyButton.gameObject);
            }
            
            Buttons.Remove(name);
            
            if (toDestroyButton != null && toDestroyButton.IsDefaultRectTransform)
            {
                ContainerUI.RefreshDefaultRectTransformButtons();
            }
        }

        public static void ClearButtons()
        {
            foreach (var button in Buttons.Values)
            {
                if (button != null) Object.Destroy(button.gameObject);
            }
            Buttons.Clear();
            ContainerUI.RefreshDefaultRectTransformButtons();
        }
    }
}