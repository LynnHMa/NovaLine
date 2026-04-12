using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.UI;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    [Serializable]
    public class DialogAction : NovaAction
    {
        private DialogUI dialogUI => DialogUI.Instance;
        public Sprite avatar;

        public string characterName;
        [TextArea]
        public string content;
        public bool showInstantly = true;
        
        [ShowInInspectorIf(nameof(showInstantly),false)]
        public float speed = 50;

        protected override IEnumerator OnInvoke()
        {
            dialogUI?.gameObject.SetActive(true);
            yield return dialogUI?.ShowDialogueCoroutine(avatar,characterName,content,showInstantly ? 0 : speed);
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Dialog Action]";
        }

    }
}
