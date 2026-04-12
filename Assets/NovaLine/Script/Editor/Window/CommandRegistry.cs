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
        private int _compoundDepth;
        
        public readonly Stack<Command.Command> undoStack = new();
        public readonly Stack<Command.Command> redoStack = new();
        public bool IsImporting { get; set; }
        private Stack<Command.Command> PreparativeStack { get; } = new();
        public bool IsRecordingCompoundCommand { get; set; }
        private List<Command.Command> RecordedCommands { get; } = new();
        
        public static CommandRegistry Instance => CurrentGraphViewNodeContext?.CommandRegistry;
        public static Stack<Command.Command> UndoStack => Instance?.undoStack;
        public static Stack<Command.Command> RedoStack => Instance?.redoStack;

        public void ImportPreparativeStack()
        {
            if (IsImporting) return;

            IsImporting = true;

            EditorApplication.delayCall += () =>
            {
                switch (PreparativeStack.Count)
                {
                    case >= 2:
                    {
                        var toImports = new List<Command.Command>();
                        while (PreparativeStack.Count > 0)
                        {
                            var toImport = PreparativeStack.Pop();
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
                        undoStack.Push(PreparativeStack.Pop());
                        break;
                }

                IsImporting = false;
            };
        }
        public void Register(Command.Command command)
        {
            if (IsRecordingCompoundCommand)
            {
                //Debug.Log("[Register Recorded Compound Command] " + command.type);
                RecordedCommands.Add(command);
            }
            else
            {
                //Debug.Log("[Register Command] " + command.type);
                
                PreparativeStack?.Push(command);

                if (PreparativeStack?.Count != 0 && !IsImporting)
                {
                    ImportPreparativeStack();
                }
                
                redoStack?.Clear();
            }
        }

        public void BeginRecordingCompoundCommand()
        {
            if (_compoundDepth == 0)
            {
                RecordedCommands.Clear(); // 只在最外层才清空
            }
            _compoundDepth++;
            IsRecordingCompoundCommand = true;
        }

        public void EndRecordingCompoundCommand()
        {
            _compoundDepth--;
            if (_compoundDepth > 0) return; // 还在外层 scope 里，不提前结束

            IsRecordingCompoundCommand = false;
            if (RecordedCommands.Count > 0)
            {
                var compoundCommand = new CompoundCommand(new List<Command.Command>(RecordedCommands));
                RecordedCommands.Clear();
                Register(compoundCommand);
            }
        }
        public static void RegisterCommand(Command.Command command)
        {
            if (command == null) return;
            Instance?.Register(command);
        }
        public static void Undo()
        {
            if (Instance == null || UndoStack?.Count == 0) return;

            var undo = UndoStack?.Pop();
            if (undo != null)
            {
                undo.Undo();
                RedoStack?.Push(undo);
            }
        }
        public static void Redo()
        {
            if (Instance == null || RedoStack?.Count == 0) return;

            var redo = RedoStack?.Pop();
            if (redo != null)
            {
                redo.Redo();
                UndoStack?.Push(redo);
            }
        }

        [Shortcut("NovaLine/Undo", typeof(NovaWindow), KeyCode.Z, ShortcutModifiers.Action)]
        private static void UndoOverride()
        {
            if (TryCustomUndo()) return;

            UnityEditor.Undo.PerformUndo();
        }

        [Shortcut("NovaLine/Redo", typeof(NovaWindow), KeyCode.Y, ShortcutModifiers.Action)]
        private static void RedoOverride()
        {
            if (TryCustomRedo()) return;

            UnityEditor.Undo.PerformRedo();
        }
        private static bool TryCustomUndo()
        {
            if (CurrentGraphViewNodeContext != null)
            {
                Undo();
                return true;
            }
            return false;
        }
        private static bool TryCustomRedo()
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
