using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI.Container
{
    public class DialogContainerUI : MonoBehaviour
    {
        public static DialogContainerUI Instance { get; private set; }
        public static Coroutine ShowingCoroutine { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public Image avatar;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI contentText;
        
        private void Awake()
        {
            Instance = this;
            CanvasGroup = GetComponent<CanvasGroup>();
            CanvasGroup.alpha = 0f;
        }

        public IEnumerator ShowUI()
        {
            ClearContent();
            CanvasGroup.alpha = 0f;
            while (CanvasGroup.alpha < 1f)
            {
                CanvasGroup.alpha += Time.deltaTime * 1f / NovaPlayer.Instance.fadeInDuration;
                yield return null;
            }
            CanvasGroup.alpha = 1f;
        }
        public IEnumerator HideUI()
        {
            if (ShowingCoroutine != null)
            {
                StopCoroutine(ShowingCoroutine);
            }
            while (CanvasGroup.alpha > 0f)
            {
                CanvasGroup.alpha -= Time.deltaTime * 1f / NovaPlayer.Instance.fadeOutDuration;
                yield return null;
            }
            CanvasGroup.alpha = 0f;
            ClearContent();
        }

        public void ClearContent()
        {
            nameText.text = "";
            contentText.text = "";
            avatar.sprite = null;
        }

        public IEnumerator ShowDialogueCoroutine(Sprite avatarSprite, string name, string content,float showingSpeed = 0)
        {
            if(CanvasGroup.alpha < 1f) yield return ShowUI();
            
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
    }
}
