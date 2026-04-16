using System;
using System.Collections;
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
            if(_setSpriteRoutine != null) StopCoroutine(_setSpriteRoutine);
            _setSpriteRoutine = StartCoroutine(SetSpriteRoutine(sprite));
        }

        public IEnumerator SetSpriteRoutine(Sprite sprite)
        {
            yield return new WaitForSeconds(0.05f);
            SpriteRenderer.sprite = sprite;
        }
    }
}
