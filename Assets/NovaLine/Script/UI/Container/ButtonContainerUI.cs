using System.Collections.Generic;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.UI.Container
{
public class ButtonContainerUI : MonoBehaviour
    {
        public static ButtonContainerUI Instance { get; private set; }
        public Dictionary<string, NovaButton> Buttons { get; } = new();
        private RectTransform RectTransform { get; set; }
        
        public float spacing = 10f;
        public float topPadding = 10f;
        public float bottomPadding = 10f;
        public float maxHeight = 1080f;

        private void Awake()
        {
            Instance = this;
            RectTransform = GetComponent<RectTransform>();
            
            RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            RectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        public static NovaButton GetButton(string name)
        {
            return Instance.Buttons.GetValueOrDefault(name);
        }
        
        public static NovaButton RegisterButton(string displayName, NovaButton buttonPrefab, RectTransformChecker rectTransformChecker = null)
        {
            var instantiatedButton = Instantiate(buttonPrefab, Instance.transform, false);
            
            if (instantiatedButton.text != null) instantiatedButton.text.text = displayName;
            Instance.Buttons.TryAdd(displayName, instantiatedButton);
            
            instantiatedButton.IsDefaultRectTransform = rectTransformChecker == null;
            
            if (rectTransformChecker != null)
            {
                rectTransformChecker.LinkedTransform = instantiatedButton.transform;
                rectTransformChecker.ExportToTransform();
            }
            else
            {
                Instance.RefreshDefaultRectTransformButtons();
            }
            return instantiatedButton;
        }

        public static void UnregisterButton(string name)
        {
            if (!Instance.Buttons.TryGetValue(name, out var toDestroyButton)) return;
            
            if (toDestroyButton != null && toDestroyButton.gameObject != null) 
            {
                Destroy(toDestroyButton.gameObject);
            }
            
            Instance.Buttons.Remove(name);
            
            if (toDestroyButton != null && toDestroyButton.IsDefaultRectTransform)
            {
                Instance.RefreshDefaultRectTransformButtons();
            }
        }

        public static void ClearButtons()
        {
            foreach (var button in Instance.Buttons.Values)
            {
                if (button != null) Destroy(button.gameObject);
            }
            Instance.Buttons.Clear();
            Instance.RefreshDefaultRectTransformButtons();
        }
        
        public void RefreshDefaultRectTransformButtons()
        {
            Canvas.ForceUpdateCanvases();
            var validButtons = new List<NovaButton>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var btn = transform.GetChild(i).GetComponent<NovaButton>();
                if (btn != null && btn.RectTransform != null && btn.IsDefaultRectTransform)
                {
                    validButtons.Add(btn);
                }
            }

            int count = validButtons.Count;
            if (count == 0)
            {
                SetContainerHeight(0);
                return;
            }
            foreach (var btn in validButtons)
            {
                var rt = btn.RectTransform;
                Vector2 realSize = rt.rect.size; 
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = realSize; 
            }
            float sumButtonHeights = 0f;
            foreach (var btn in validButtons)
            {
                sumButtonHeights += btn.RectTransform.rect.height;
            }
            float unscaledContentHeight = topPadding + bottomPadding + sumButtonHeights + (count - 1) * spacing;
            float scale = 1f;
            if (maxHeight > 0 && unscaledContentHeight > maxHeight)
            {
                scale = maxHeight / unscaledContentHeight; 
            }

            float actualContentHeight = unscaledContentHeight * scale;
            
            float currentTopY = (actualContentHeight / 2f) - (topPadding * scale);

            foreach (var btn in validButtons)
            {
                var rt = btn.RectTransform;
                rt.localScale = new Vector3(scale, scale, 1f);
                float halfScaledHeight = (rt.rect.height * scale) / 2f;
                float targetY = currentTopY - halfScaledHeight;
                rt.anchoredPosition = new Vector2(0, targetY);
                currentTopY = currentTopY - (rt.rect.height * scale) - (spacing * scale);
            }
            
            SetContainerHeight(actualContentHeight);
        }
        
        private void SetContainerHeight(float height)
        {
            if (RectTransform != null)
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
        }
    }
}