using NovaLine.Script.Editor.Window.Command;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window
{
    public class CommandRegistry
    {
        public Stack<Command.Command> undoStack { get; } = new();
        public Stack<Command.Command> redoStack { get; } = new();
        public bool isImporting { get; set; }
        private Stack<Command.Command> preparativeStack { get; } = new();
        public bool isRecordingCompoundCommand { get; set; }
        private List<Command.Command> recordedCommands { get; } = new();
        private int _compoundDepth;
        
        public static CommandRegistry Instance => CurrentGraphViewNodeContext?.commandRegistry;
        public static Stack<Command.Command> UndoStack => Instance?.undoStack;
        public static Stack<Command.Command> RedoStack => Instance?.redoStack;

        public void importPreparativeStack()
        {
            if (isImporting) return;

            isImporting = true;

            EditorApplication.delayCall += () =>
            {
                switch (preparativeStack.Count)
                {
                    case >= 2:
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
                        break;
                    }
                    case 1:
                        undoStack.Push(preparativeStack.Pop());
                        break;
                }

                isImporting = false;
            };
        }
        public void register(Command.Command command)
        {
            if (isRecordingCompoundCommand)
            {
                //Debug.Log("[Register Recorded Compound Command] " + command.type);
                recordedCommands.Add(command);
            }
            else
            {
                //Debug.Log("[Register Command] " + command.type);
                
                preparativeStack?.Push(command);

                if (preparativeStack?.Count != 0 && !isImporting)
                {
                    importPreparativeStack();
                }
                
                redoStack?.Clear();
            }
        }

        public void beginRecordingCompoundCommand()
        {
            if (_compoundDepth == 0)
            {
                recordedCommands.Clear(); // 只在最外层才清空
            }
            _compoundDepth++;
            isRecordingCompoundCommand = true;
        }

        public void endRecordingCompoundCommand()
        {
            _compoundDepth--;
            if (_compoundDepth > 0) return; // 还在外层 scope 里，不提前结束

            isRecordingCompoundCommand = false;
            if (recordedCommands.Count > 0)
            {
                var compoundCommand = new CompoundCommand(new List<Command.Command>(recordedCommands));
                recordedCommands.Clear();
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
            if (CurrentGraphViewNodeContext != null)
            {
                Undo();
                return true;
            }
            return false;
        }
        private static bool tryCustomRedo()
        {
            if (CurrentGraphViewNodeContext != null)
            {
                Redo();
                return true;
            }
            return false;
        }
    }
}
