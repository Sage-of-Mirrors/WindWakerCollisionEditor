using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    interface IRenderable
    {
        void Render(int _uniformMVP, int _uniformColor, Matrix4 viewMatrix, Matrix4 projMatrix);
    }

    interface IPickable
    {
        bool IsRayColliding(Vector3 eye, Vector3 ray);
    }
}
