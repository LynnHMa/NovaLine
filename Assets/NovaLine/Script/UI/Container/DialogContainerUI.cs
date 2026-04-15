using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI.Container
{
    public class DialogContainerUI : MonoBehaviour
    {
        public static DialogContainerUI Instance { get;private set; }
        public Coroutine ShowingCoroutine { get; set; }
        public Image avatar;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI contentText;
        
        private void Awake()
        {
            Instance = this;
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        public void HideUI()
        {
            StopCoroutine(ShowingCoroutine);
            gameObject.SetActive(false);
        }

        public void ClearContent()
        {
            nameText.text = "";
            contentText.text = "";
            avatar.sprite = null;
        }

        public IEnumerator ShowDialogueCoroutine(Sprite avatarSprite, string name, string content,float showingSpeed = 0)
        {
            ClearContent();
            ShowUI();
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
