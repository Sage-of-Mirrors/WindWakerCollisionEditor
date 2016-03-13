using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindWakerCollisionEditor
{
    class SelectTriangleEventArgs : EventArgs
    {
        public SelectTriangleEventArgs()
        {
            SelectedTris = new List<Triangle>();
        }

        public List<Triangle> SelectedTris { get; set; }
    }
}
