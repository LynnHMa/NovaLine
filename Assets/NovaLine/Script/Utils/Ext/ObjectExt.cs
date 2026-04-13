using UnityEngine;

namespace NovaLine.Script.Utils.Ext
{
    public static class ObjectExt
    {
        public static T Instantiate<T>(this T original, Vector3 position, Vector3 scale, Quaternion rotation,
            Transform parent = null) where T : Object
        {
            var obj = Object.Instantiate(original, position, rotation, parent);
            Transform transform = obj switch
            {
                GameObject gameObject => gameObject.transform,
                Component component => component.transform,
                _ => null
            };

            if (transform != null)
            {
                transform.localScale = scale;
            }

            return obj;
        }
    }
}