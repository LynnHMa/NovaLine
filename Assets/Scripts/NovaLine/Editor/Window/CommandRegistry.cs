using NovaLine.Editor.Window.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Window
{
    public class CommandRegistry
    {
        public Stack<Command.Command> undoStack { get; set; } = new();
        public Stack<Command.Command> redoStack { get; set; } = new();
        public bool isImporting { get; set; }
        private Stack<Command.Command> preparativeStack { get; set; } = new();
        public bool isRecordingCompoundCommand { get; set; }
        private List<Command.Command> recordedCommands { get; set; } = new();

        public static CommandRegistry Instance => CurrentGraphViewContext?.commandRegistry;
        public static Stack<Command.Command> UndoStack => Instance?.undoStack;
        public static Stack<Command.Command> RedoStack => Instance?.redoStack;

        private async Task importPreparativeStackTask()
        {
            if (isImporting) return;

            isImporting = true;

            await Task.Delay(100);

            if (preparativeStack.Count >= 2)
            {
                var toImports = new List<Command.Command>();
                while (preparativeStack.Count > 0)
                {
                    var toImport = preparativeStack.Pop();
                    if (toImport != null)
                    {
                        toImports.Add(toImport);
                    }
                }
                var compoundCommand = new CompoundCommand(toImports);
                undoStack.Push(compoundCommand);
            }
            else if (preparativeStack.Count == 1)
            {
                undoStack.Push(preparativeStack.Pop());
            }

            isImporting = false;
        }
        public void importPreparativeStack()
        {
            Task.Run(importPreparativeStackTask);
        }
        public void register(Command.Command command)
        {
            if (isRecordingCompoundCommand)
            {
                Debug.Log("[Register Recorded Compound Command] " + command.type);
                recordedCommands.Add(command);
            }
            else
            {
                Debug.Log("[Register Command] " + command.type);
                
                preparativeStack?.Push(command);

                if (preparativeStack?.Count != 0 && !isImporting)
                {
                    importPreparativeStack();
                }
            }
        }
        public void beginRecordingCompoundCommand()
        {
            isRecordingCompoundCommand = true;
            recordedCommands.Clear();
        }
        public void endRecordingCompoundCommand()
        {
            isRecordingCompoundCommand = false;
            if(recordedCommands.Count > 0)
            {
                var compoundCommand = new CompoundCommand(recordedCommands);
                register(compoundCommand);
            }
        }
        public static void Register(Command.Command command)
        {
            if (command == null) return;
            Instance?.register(command);
        }
        public static void Undo()
        {
            if (Instance == null || UndoStack?.Count == 0) return;

            var undo = UndoStack?.Pop();
            if (undo != null)
            {
                undo.undo();
                RedoStack?.Push(undo);
            }
        }
        public static void Redo()
        {
            if (Instance == null || RedoStack?.Count == 0) return;

            var redo = RedoStack?.Pop();
            if (redo != null)
            {
                redo.redo();
                UndoStack?.Push(redo);
            }
        }

        [Shortcut("NovaLine/Undo", typeof(NovaWindow), KeyCode.Z, ShortcutModifiers.Action)]
        private static void undoOverride()
        {
            if (tryCustomUndo()) return;

            UnityEditor.Undo.PerformUndo();
        }

        [Shortcut("NovaLine/Redo", typeof(NovaWindow), KeyCode.Y, ShortcutModifiers.Action)]
        private static void redoOverride()
        {
            if (tryCustomRedo()) return;

            UnityEditor.Undo.PerformRedo();
        }
        private static bool tryCustomUndo()
        {
            if (CurrentGraphViewContext != null)
            {
                Undo();
                return true;
            }
            return false;
        }
        private static bool tryCustomRedo()
        {
            if (CurrentGraphViewContext != null)
            {
                Redo();
                return true;
            }
            return false;
        }
    }
}
