using System;
using UnityEngine;
using NovaLine.Element;
using System.Threading.Tasks;

namespace NovaLine.Action
{
    [Serializable]
    public class DialogAction : NovaAction
    {
        public Character character;
        [TextArea]
        public string content;

        public override Task invoke()
        {
            //todo character dialogue.
            return base.invoke();
        }

        public override string getType()
        {
            return "[Dialog Action]";
        }

    }
}
