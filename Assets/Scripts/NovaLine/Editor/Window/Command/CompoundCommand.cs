using System.Collections.Generic;
using System;
using UnityEngine;

namespace NovaLine.Editor.Window.Command
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
                    Debug.Log("WTF Compound!");
                    this.commands.AddRange(compoundCommand.commands);
                    commands.RemoveAt(i);
                }
            }
            
            this.commands.AddRange(commands);

            mergeCongenericCommand();
        }
        public override void undo(bool autoSave = true)
        {
            for (var i = commands.Count - 1; i >= 0; i--)
            {
                var command = commands[i];
                Debug.Log($"{i} is {command.type}");
                command.undo(false);
            }
            base.undo(autoSave);
        }
        public override void redo(bool autoSave = true)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                command.redo(false);
            }
            base.redo(autoSave);
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
