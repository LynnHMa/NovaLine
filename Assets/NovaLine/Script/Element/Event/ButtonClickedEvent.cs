using System.Collections;
using NovaLine.Script.UI;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    public class ButtonClickedEvent : NovaEvent
    {
        private const string DEFAULT_BUTTON_PREFAB_PATH = "Prefab/UI/Example Button";
        public string displayName;
        public NovaButton buttonPrefab;
        
        public bool defaultRectTransform = true;
        [ShowInInspectorIf(nameof(defaultRectTransform),false)]
        public RectTransformChecker rectTransformChecker;

        public ButtonClickedEvent()
        {
        }
        public ButtonClickedEvent(string name, string displayName) : this()
        {
            this.name = name;
            this.displayName = displayName;
        }

        public override IEnumerator OnEvent()
        {
            var listenedButton = ButtonContainerUI.RegisterButton(displayName, buttonPrefab, defaultRectTransform ? null : rectTransformChecker);
            while (!listenedButton.isClicked)
            {
                yield return null;
            }
            
            yield return null;
            
            yield return base.OnEvent();
        }
        public override string GetTypeName()
        {
            return "[Button Clicked Event]";
        }
    }
}