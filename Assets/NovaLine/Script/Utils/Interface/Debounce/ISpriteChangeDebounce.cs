using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Utils.Interface.Debounce
{
    public interface ISpriteChangeDebounce
    {
        void SetSpriteDebounce(Sprite sprite);
        IEnumerator SetSpriteRoutine(Sprite sprite);
    }
}