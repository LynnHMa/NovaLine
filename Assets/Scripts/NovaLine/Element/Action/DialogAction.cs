using System;
using System.Threading.Tasks;
using NovaLine.Action;
using UnityEngine;

namespace NovaLine.Element.Action
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

        public override string getTypeName()
        {
            return "[Dialog Action]";
        }

    }
}
