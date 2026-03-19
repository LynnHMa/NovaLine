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

        public DialogAction() : base()
        {
        }

        public override async Task invoke()
        {
            //todo character dialogue.
            await base.invoke();
        }

        public override string getType()
        {
            return "[Dialog Action]";
        }

    }
}
