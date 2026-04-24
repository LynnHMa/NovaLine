using System.Collections;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface.Debounce;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI.Container
{
    public class DialogContainerUI : NovaContainerUI,IGameObjectActiveDebounce
    {
        public static DialogContainerUI Instance { get; private set; }
        public Coroutine HideShowCoroutine { get; set; }
        public Image avatar;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI contentText;
        
        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }

        public void ClearContent()
        {
            nameText.text = "";
            contentText.text = "";
            avatar.sprite = null;
        }

        public IEnumerator ShowDialogueCoroutine(Sprite avatarSprite, string name, string content,float showingSpeed = 0)
        {
            if (CanvasGroup.alpha < 1f) yield return HideShowDebounceRoutine(true);
            
            avatar.sprite = avatarSprite;
            nameText.text = name;
            
            if (showingSpeed == 0)
            {
                contentText.text = content;
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                var waitDelay = new WaitForSeconds(1f / showingSpeed);
                
                for (var i = 0; i < content.Length; i++)
                {
                    //Click to skip
                    if (Input.GetMouseButtonDown(0))
                    {
                        contentText.text = content;
                        yield return null;
                        break;
                    }
                    
                    contentText.text = content.Substring(0, i + 1);
                    yield return waitDelay;
                }
            }
            
            yield return null;
        }
        public void InactiveDebounce()
        {
            HideShowCoroutine.StopCoroutine();
            HideShowDebounceRoutine(false).StartCoroutine();
        }

        public void ActiveDebounce()
        {
            HideShowCoroutine.StopCoroutine();
            HideShowDebounceRoutine(true).StartCoroutine();
        }

        public IEnumerator HideShowDebounceRoutine(bool isActive)
        {
            var isFade = isActive ? NovaPlayer.Instance.fadeIn : NovaPlayer.Instance.fadeOut;
            var fadeDuration = isActive ? NovaPlayer.Instance.fadeInDuration : NovaPlayer.Instance.fadeOutDuration;
            
            yield return new WaitForSeconds(fadeDuration + 0.05f);
            
            if(isActive) ClearContent();
            
            if (isFade)
            {
                CanvasGroup.alpha = isActive ? 0f : 1f;
                while (CanvasGroup.alpha < 1f)
                {
                    CanvasGroup.alpha += Time.deltaTime * (isActive ? 1f : -1f) / fadeDuration;
                    yield return null;
                }
            }

            CanvasGroup.alpha = isActive ? 1f : 0f;
            if(!isActive) ClearContent();
        }
    }
}
