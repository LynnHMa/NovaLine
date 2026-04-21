using UnityEngine;

namespace NovaLine.Script.Utils.Ext
{
    public static class ColorExt
    {
        /// <summary>
        /// Red
        /// </summary>
        public static readonly Color NODE_THEMED_COLOR = Color.red;

        /// <summary>
        /// Orange
        /// </summary>
        public static readonly Color ACTION_THEMED_COLOR = new (1f, 130f / 255f, 0f);

        /// <summary>
        /// Pink
        /// </summary>
        public static readonly Color EVENT_THEMED_COLOR = new (1f, 130f / 255f, 1f);
        
        /// <summary>
        /// Green
        /// </summary>
        public static readonly Color CONDITION_THEMED_COLOR = new (0.4f, 1f, 0.4f);
        
        public static readonly Color LIGHT_RED = new (1f, 0.4f, 0.4f);
    }
}
