using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    /// <summary>
    /// Change a new background
    /// </summary>
    [Serializable]
    public class BackgroundAction : NovaAction
    {
        [Tooltip("Background sprite you want")]
        public Sprite sprite;
        public TransformChecker transform;
        protected override IEnumerator OnInvoke()
        {
            var background = NovaPlayer.Instance?.background;
            if (background != null)
            {
                transform.LinkedTransform = background.transform;
                transform?.ExportToTransform();
                background.SetSpriteDebounce(sprite);
            }
            
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Background Action]";
        }
    }
}