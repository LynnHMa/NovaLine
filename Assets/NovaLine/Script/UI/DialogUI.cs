using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NovaLine.Script.UI
{
    public class DialogUI : MonoBehaviour
    {
        public static DialogUI Instance { get;private set; }
        public Image avatar;
        public Text nameText;
        public Text contentText;
        
        public Coroutine showingCoroutine { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void showUI()
        {
            gameObject.SetActive(true);
        }
        public void hideUI()
        {
            StopCoroutine(showingCoroutine);
            gameObject.SetActive(false);
        }

        public void clearContent()
        {
            nameText.text = "";
            contentText.text = "";
            avatar.sprite = null;
        }

        public IEnumerator showDialogueCoroutine(Sprite avatarSprite, string name, string content,float showingSpeed = 0)
        {
            clearContent();
            showUI();
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
