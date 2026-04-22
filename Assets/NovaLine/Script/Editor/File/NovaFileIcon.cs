using NovaLine.Script.Element;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.Editor.File
{
    public static class NovaFileIcon
    {
        private static Texture2D _flowchartIcon;
        private static Texture2D _nodeIcon;
        private static Texture2D _actionIcon;
        private static Texture2D _conditionIcon;
        private static Texture2D _eventIcon;
        
        public static Texture2D FLOWCHART_ICON => _flowchartIcon ??= LoadIconByName("flowchart_icon");
        public static Texture2D NODE_ICON      => _nodeIcon      ??= LoadIconByName("node_icon");
        public static Texture2D ACTION_ICON    => _actionIcon    ??= LoadIconByName("action_icon");
        public static Texture2D CONDITION_ICON => _conditionIcon ??= LoadIconByName("condition_icon");
        public static Texture2D EVENT_ICON     => _eventIcon     ??= LoadIconByName("event_icon");
        
        private static Texture2D LoadIconByName(string iconName) => AssetDatabaseExt.LoadAssetByName<Texture2D>(iconName);
        
        public static Texture2D GetIcon(NovaElementType type)
        {
            return type switch
            {
                NovaElementType.Flowchart => FLOWCHART_ICON,
                NovaElementType.Node => NODE_ICON,
                NovaElementType.Action => ACTION_ICON,
                NovaElementType.Condition => CONDITION_ICON,
                NovaElementType.Event => EVENT_ICON,
                _ => null
            };
        }
    }
}