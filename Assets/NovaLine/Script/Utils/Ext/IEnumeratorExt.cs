using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovaLine.Script.Utils.Ext
{
    public static class IEnumeratorExt
    {
        private static NovaPlayer Player => NovaPlayer.Instance;
        public static IEnumerator WhenAll(this IEnumerable<IEnumerator> routines)
        {
            var routineArray = routines as IEnumerator[] ?? routines.ToArray();
            
            if(Player == null || !routineArray.Any()) yield break;
            
            var remaining = routineArray.Count();

            foreach (var routine in routineArray)
            {
                routine.WrapCoroutine(() => remaining--).StartCoroutine();
            }
            
            yield return new WaitUntil(() => remaining <= 0);
        }

        public static IEnumerator WhenAny(this IEnumerable<IEnumerator> routines)
        {
            var routineArray = routines as IEnumerator[] ?? routines.ToArray();
            
            if (Player == null || !routineArray.Any()) yield break;
            
            var isAnyCompleted = false;
            var activeCoroutines = new List<Coroutine>();
            
            foreach (var routine in routineArray)
            {
                var coroutine = routine.WrapCoroutine(() => isAnyCompleted = true).StartCoroutine();
                activeCoroutines.Add(coroutine);
            }
            
            yield return new WaitUntil(() => isAnyCompleted);
            
            foreach (var coroutine in activeCoroutines)
            {
                coroutine.StopCoroutine();
            }
        }

        public static Coroutine StartCoroutine(this IEnumerator routine)
        {
            if (routine != null && Player != null)
            {
                return Player.StartCoroutine(routine);
            }

            return null;
        }

        public static void StopCoroutine(this Coroutine routine)
        {
            if (routine != null && Player != null)
            {
                Player.StopCoroutine(routine);
            }
        }

        private static IEnumerator WrapCoroutine(this IEnumerator routine, System.Action onComplete)
        {
            yield return routine;
            onComplete?.Invoke();
        }
    }
}
