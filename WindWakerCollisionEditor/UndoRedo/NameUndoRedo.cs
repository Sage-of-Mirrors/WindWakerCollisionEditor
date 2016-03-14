using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    public class NameUndoRedo : IUndoRedoCommand
    {
        /// <summary>
        /// Original name of the object
        /// </summary>
        string oldName;
        /// <summary>
        /// New name of the object
        /// </summary>
        string newName;
        /// <summary>
        /// Object that was renamed
        /// </summary>
        object renamedObj;
        /// <summary>
        /// Instantiates a new NameUndoRedo command.
        /// </summary>
        /// <param name="nameOld">Old name of the object</param>
        /// <param name="nameNew">New name of the object</param>
        /// <param name="renamed">Renamed object</param>
        public NameUndoRedo(string nameOld, string nameNew, object renamed)
        {
            oldName = nameOld;
            newName = nameNew;
            renamedObj = renamed;
        }

        public void Execute()
        {
            if (renamedObj.GetType() == typeof(Category))
            {
                Category cat = renamedObj as Category;
                cat.SetName(newName);
            }
            else if (renamedObj.GetType() == typeof(Group))
            {
                Group grp = renamedObj as Group;
                grp.Name = newName;
            }
        }

        public void UnExecute()
        {
            if (renamedObj.GetType() == typeof(Category))
            {
                Category cat = renamedObj as Category;
                cat.SetName(oldName);
            }
            else if (renamedObj.GetType() == typeof(Group))
            {
                Group grp = renamedObj as Group;
                grp.Name = oldName;
            }
        }
    }
}
