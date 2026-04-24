using System.Collections;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface.Debounce;
using UnityEngine;

namespace NovaLine.Script
{
    public class Background : MonoBehaviour,ISpriteChangeDebounce
    {
        public SpriteRenderer SpriteRenderer { get; private set; }
        private Coroutine _setSpriteRoutine;
        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        }

        public void SetSpriteDebounce(Sprite sprite)
        {
            _setSpriteRoutine.StopCoroutine();
            SetSpriteRoutine(sprite).StartCoroutine();
        }

        public IEnumerator SetSpriteRoutine(Sprite sprite)
        {
            yield return new WaitForSeconds(0.05f);
            SpriteRenderer.sprite = sprite;
        }
    }
}
