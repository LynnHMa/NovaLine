using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    /// <summary>
    /// Action of controlling NovaPlayer audio source.
    /// </summary>
    [Serializable]
    public class AudioAction : NovaAction
    {
        private static AudioSource AudioSource => NovaPlayer.Instance.audioSource;
        public enum AudioActionType
        {
            Play,
            Stop,
            Pause,
            Resume,
        }
        
        public AudioActionType audioActionType;
        
        [ShowInInspectorIf(nameof(audioActionType),AudioActionType.Play)]
        public bool loop;

        [ShowInInspectorIf(nameof(audioActionType),AudioActionType.Play)]
        public float volume = 1f;

        [ShowInInspectorIf(nameof(audioActionType), AudioActionType.Play)]
        public float pitch = 1f;
        
        [ShowInInspectorIf(nameof(audioActionType),AudioActionType.Play)]
        public AudioClip audioClip;
        
        protected override IEnumerator OnInvoke()
        {
            if (AudioSource != null)
            {
                switch (audioActionType)
                {
                    case AudioActionType.Play:
                        AudioSource.clip = audioClip;
                        AudioSource.loop = loop;
                        AudioSource.volume = volume;
                        AudioSource.pitch = pitch;
                        AudioSource.Play();
                        break;
                    case AudioActionType.Stop:
                        AudioSource.clip = null;
                        AudioSource.Stop();
                        break;
                    case AudioActionType.Pause:
                        AudioSource.Pause();
                        break;
                    case  AudioActionType.Resume:
                        AudioSource.UnPause();
                        break;
                }
            }
            else Debug.LogWarning("NovaPlayer AudioSource is null!");
            
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Audio Action]";
        }
    }
}