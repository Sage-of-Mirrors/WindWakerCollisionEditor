using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    interface IModelSource
    {
        IEnumerable<Category> Load(string fileName);
    }
}
