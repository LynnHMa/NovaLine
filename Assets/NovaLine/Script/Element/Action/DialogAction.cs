using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.UI;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    [Serializable]
    public class DialogAction : NovaAction
    {
        public Character character;

        [TextArea]
        public string content;
        [Header("Number of content characters displayed per second,set zero for instantly displaying.")]
        public float speed = 50;

        protected override IEnumerator onInvoke()
        {
            yield return DialogUI.Instance.showCharacterDialogueCoroutine(character,content,speed);
            yield return base.onInvoke();
        }

        public override string getTypeName()
        {
            return "[Dialog Action]";
        }

    }
}
