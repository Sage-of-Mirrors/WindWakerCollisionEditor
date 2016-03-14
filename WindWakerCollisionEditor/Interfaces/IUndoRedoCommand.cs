using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    public interface IUndoRedoCommand
    {
        /// <summary>
        /// Redo an action
        /// </summary>
        void Execute();
        /// <summary>
        /// Undo an action
        /// </summary>
        void UnExecute();
    }
}
