using System.Collections.Generic;
using UnityEngine;

namespace NovaLine.Script.UI.Container
{
    public class ButtonContainerUI : NovaContainerUI
    {
        public static ButtonContainerUI Instance { get; private set; }
        
        public float spacing = 10f;
        public float topPadding = 10f;
        public float bottomPadding = 10f;
        public float maxHeight = 1080f;

        protected override void Awake()
        {
            Instance = this;
            base.Awake();
            RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            RectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        public void RefreshDefaultRectTransformButtons()
        {
            Canvas.ForceUpdateCanvases();
            var validButtons = new List<OptionButton>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var btn = transform.GetChild(i).GetComponent<OptionButton>();
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
            if (RectTransform != null) RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
        }
    }
}