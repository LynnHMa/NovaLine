using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Utils.Interface.Debounce
{
    public interface IGameObjectActiveDebounce
    {
        Coroutine HideShowCoroutine { get; set; }
        void InactiveDebounce();
        void ActiveDebounce();
        IEnumerator HideShowDebounceRoutine(bool isActive);
    }
}