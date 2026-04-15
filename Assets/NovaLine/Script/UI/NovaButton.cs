using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI
{
    public class NovaButton : MonoBehaviour
    {
        public bool isClicked;

        public TextMeshProUGUI text;
        public RectTransform RectTransform { get; set; }
        public Button Button { get; set; }
        public Image Image { get; set; }
        public CanvasRenderer CanvasRenderer { get; set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            Button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            Image = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            CanvasRenderer = GetComponent<CanvasRenderer>() ?? gameObject.AddComponent<CanvasRenderer>();
            Button.onClick.AddListener(OnClicked);
            text.rectTransform.sizeDelta = RectTransform.sizeDelta;
        }

        private void OnClicked()
        {
            isClicked = true;
            gameObject.SetActive(false);
        }
    }
}
