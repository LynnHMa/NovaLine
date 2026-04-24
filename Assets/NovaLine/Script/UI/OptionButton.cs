using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI
{
    public class OptionButton : MonoBehaviour
    {
        public bool isClicked;

        public bool IsDefaultRectTransform { get; set; }
        public RectTransform RectTransform { get; private set; }
        public Button Button { get; private set; }
        public Image Image { get; private set; }
        public CanvasRenderer CanvasRenderer { get; private set; }
        public TextMeshProUGUI text;
        
        private void Awake()
        {
            void OnClicked()
            {
                isClicked = true;
                gameObject.SetActive(false);
            }
            
            RectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            Button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            Image = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            CanvasRenderer = GetComponent<CanvasRenderer>() ?? gameObject.AddComponent<CanvasRenderer>();
            Button.onClick.AddListener(OnClicked);
            text.rectTransform.sizeDelta = RectTransform.sizeDelta;
        }
    }
}
