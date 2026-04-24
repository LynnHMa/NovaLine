using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.UI;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    /// <summary>
    /// Shows or hides a dialog window.
    /// </summary>
    [Serializable]
    public class DialogAction : NovaAction
    {
        private DialogContainerUI DialogContainerUI => DialogContainerUI.Instance;
        
        public bool showDialogue = true;
        
        [ShowInInspectorIf(nameof(showDialogue),true)]
        public Sprite avatar;
        
        [ShowInInspectorIf(nameof(showDialogue),true)]
        public string characterName;
        
        [ShowInInspectorIf(nameof(showDialogue),true),TextArea]
        public string content;
        
        [ShowInInspectorIf(nameof(showDialogue),true)]
        public bool showInstantly = true;
        
        [ShowInInspectorIf(nameof(showInstantly),false),ShowInInspectorIf(nameof(showDialogue),true),Tooltip("Text displayed per second")]
        public float showingSpeed = 50;
        protected override IEnumerator OnInvoke()
        {
            if (showDialogue)
            {
                var actualShowingSpeed = showInstantly ? 0 : showingSpeed;
                yield return DialogContainerUI?.ShowDialogueCoroutine(avatar,characterName,content,actualShowingSpeed);
            }
            else
            {
                yield return DialogContainerUI.HideShowDebounceRoutine(false);
            }
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Dialog Action]";
        }

    }
}
