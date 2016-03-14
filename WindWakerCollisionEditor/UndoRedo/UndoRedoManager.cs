using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    public class UndoRedoManager
    {
        /// <summary>
        /// List of undoable actions
        /// </summary>
        List<IUndoRedoCommand> UndoList;
        /// <summary>
        /// List of redoable actions
        /// </summary>
        List<IUndoRedoCommand> RedoList;

        public UndoRedoManager()
        {
            UndoList = new List<IUndoRedoCommand>();
            RedoList = new List<IUndoRedoCommand>();
        }
        /// <summary>
        /// Undoes an action
        /// </summary>
        public void Undo()
        {
            IUndoRedoCommand cmd = UndoList.Last();
            cmd.UnExecute();
            UndoList.Remove(cmd);
            RedoList.Add(cmd);
        }
        /// <summary>
        /// Redoes an action
        /// </summary>
        public void Redo()
        {
            IUndoRedoCommand cmd = RedoList.Last();
            cmd.Execute();
            RedoList.Remove(cmd);
            UndoList.Add(cmd);
        }
        /// <summary>
        /// Adds a command to the undo list and clears RedoList
        /// </summary>
        /// <param name="cmd">Command to add to the undo list</param>
        public void AddUndoableCommand(IUndoRedoCommand cmd)
        {
            UndoList.Add(cmd);
            RedoList.Clear();
        }
        /// <summary>
        /// Clears the undo and redo lists
        /// </summary>
        public void Clear()
        {
            UndoList.Clear();
            RedoList.Clear();
        }
        /// <summary>
        /// Checks to see if the undo list contains any commands and returns false if it doesn't.
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return UndoList.Count != 0;
        }
        /// <summary>
        /// Checks to see if the redo list contains any commands and returns false if it doesn't.
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return RedoList.Count != 0;
        }
    }
}
