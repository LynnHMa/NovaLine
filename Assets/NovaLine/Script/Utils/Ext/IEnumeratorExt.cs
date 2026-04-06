using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Script.Utils.Ext
{
    public static class IEnumeratorExt
    {
        private static FlowchartPlayer Player => FlowchartPlayer.Instance;
        public static IEnumerator WhenAll(this IEnumerable<IEnumerator> routines)
        {
            var routineArray = routines as IEnumerator[] ?? routines.ToArray();
            
            if(Player == null || !routineArray.Any()) yield break;
            
            var remaining = routineArray.Count();

            foreach (var routine in routineArray)
            {
                Player.StartCoroutine(WrapCoroutine(routine,() => remaining--));
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
                var coroutine = Player.StartCoroutine(WrapCoroutine(routine,() => isAnyCompleted = true));
                activeCoroutines.Add(coroutine);
            }
            
            yield return new WaitUntil(() => isAnyCompleted);
            
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    Player.StopCoroutine(coroutine);
                }
            }
        }

        public static void StartCoroutine(this IEnumerator routine)
        {
            if (Player != null)
            {
                Player.StartCoroutine(routine);
            }
        }

        private static IEnumerator WrapCoroutine(this IEnumerator routine, System.Action onComplete)
        {
            yield return routine;
            onComplete?.Invoke();
        }
    }
}
