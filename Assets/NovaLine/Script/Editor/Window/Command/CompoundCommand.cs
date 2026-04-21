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
            Type = CommandType.Compound;
            
            foreach (var command in commands)
            {
                if (command is CompoundCommand child)
                    this.commands.AddRange(child.commands);
                else
                    this.commands.Add(command);
            }

            MergeCongenericCommand();

            if (linkedContextInfo == null && this.commands.Count > 0)
                linkedContextInfo = this.commands[0].linkedContextInfo;
        }
        public override void OnUndo()
        {
            for (var i = commands.Count - 1; i >= 0; i--)
            {
                var command = commands[i];
                command.OnUndo();
            }
        }
        public override void OnRedo()
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                command.OnRedo();
            }
        }
        //Merge congeneric command
        private void MergeCongenericCommand()
        {
            if (commands != null && commands.Count > 1)
            {
                commands.Sort((x, y) => x.Type.CompareTo(y.Type));
                var commandsStack = new Stack<Command>(commands);
                commands.Clear();
                while (commandsStack.Count > 0)
                {
                    var firstCommand = commandsStack.Pop();
                    if (firstCommand != null)
                    {
                        if (linkedContextInfo == null) linkedContextInfo = firstCommand.linkedContextInfo;
                        while (commandsStack.Count > 0 && firstCommand.Type == commandsStack.Peek().Type)
                        {
                            var selectedCommand = commandsStack.Pop();
                            if (selectedCommand != null) firstCommand.Merge(selectedCommand);
                        }
                    }
                    commands.Add(firstCommand);
                }
            }
        }

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not CompoundCommand compoundCommand) return;
            commands.AddRange(compoundCommand.commands);
            MergeCongenericCommand();
        }
    }
}
