using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenGL_in_WPF_Framework.Interfaces
{
    interface IPickable
    {
        bool IsRayColliding(Vector3 eye, Vector3 ray);
    }
}
