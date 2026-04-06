using System.Collections.Generic;
using System;
using UnityEngine;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class CompoundCommand : Command
    {
        public List<Command> commands = new();
        public CompoundCommand(List<Command> commands)
        {
            type = CommandType.Compound;
            
            //Dismantle compound child command
            for (var i = commands.Count - 1; i >= 0; i--)
            {
                var command = commands[i];
                if (command is CompoundCommand compoundCommand)
                {
                    this.commands.AddRange(compoundCommand.commands);
                    commands.RemoveAt(i);
                }
            }
            
            this.commands.AddRange(commands);

            mergeCongenericCommand();
            
            if (linkedContextInfo == null && this.commands.Count > 0)
            {
                linkedContextInfo = this.commands[0].linkedContextInfo;
            }
        }
        public override void onUndo()
        {
            for (var i = commands.Count - 1; i >= 0; i--)
            {
                var command = commands[i];
                command.onUndo();
            }
        }
        public override void onRedo()
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                command.onRedo();
            }
        }
        //Merge congeneric command
        private void mergeCongenericCommand()
        {
            if (commands != null && commands.Count > 1)
            {
                commands.Sort((x, y) => x.type.CompareTo(y.type));
                var commandsStack = new Stack<Command>(commands);
                commands.Clear();
                while (commandsStack.Count > 0)
                {
                    var firstCommand = commandsStack.Pop();
                    if (firstCommand != null)
                    {
                        if (linkedContextInfo == null) linkedContextInfo = firstCommand.linkedContextInfo;
                        while (commandsStack.Count > 0 && firstCommand.type == commandsStack.Peek().type)
                        {
                            var selectedCommand = commandsStack.Pop();
                            if (selectedCommand != null) firstCommand.merge(selectedCommand);
                        }
                    }
                    commands.Add(firstCommand);
                }
            }
        }

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not CompoundCommand compoundCommand) return;
            commands.AddRange(compoundCommand.commands);
            mergeCongenericCommand();
        }
    }
}
