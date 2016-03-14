using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    public class UndoRedoEventArgs : EventArgs
    {
        public IUndoRedoCommand cmd;
    }
}
