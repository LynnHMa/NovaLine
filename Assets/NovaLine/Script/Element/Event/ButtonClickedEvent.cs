using System;
using System.Collections;
using NovaLine.Script.Registry;
using NovaLine.Script.UI;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;

namespace NovaLine.Script.Element.Event
{
    /// <summary>
    /// Show a new button and listen for its click event.
    /// </summary>
    [Serializable]
    public class ButtonClickedEvent : NovaEvent
    {
        public string displayName;
        public OptionButton buttonPrefab;
        
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
            var listenedButton = ButtonRegistry.RegisterButton(displayName, buttonPrefab, defaultRectTransform ? null : rectTransformChecker);
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