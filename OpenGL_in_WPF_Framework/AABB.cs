using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    public class AABB
    {
        public Vector3 Max;

        public Vector3 Min;

        public Vector3 Center;
        
        public AABB()
        {
            Max = new Vector3();

            Min = new Vector3();

            Center = new Vector3();
        }

        public AABB(List<Triangle> triList)
        {
            float maxX = float.MinValue;

            float maxY = float.MinValue;

            float maxZ = float.MinValue;

            float minX = float.MaxValue;

            float minY = float.MaxValue;

            float minZ = float.MaxValue;

            Console.WriteLine("Triangle count: " + triList.Count);

            foreach (Triangle tri in triList)
            {
                #region Vert1
                if (tri.Vertex1.X > maxX)
                    maxX = tri.Vertex1.X;

                if (tri.Vertex1.Y > maxY)
                    maxY = tri.Vertex1.Y;

                if (tri.Vertex1.Z > maxZ)
                    maxZ = tri.Vertex1.Z;

                if (tri.Vertex1.X < minX)
                    minX = tri.Vertex1.X;

                if (tri.Vertex1.Y < minY)
                    minY = tri.Vertex1.Y;

                if (tri.Vertex1.Z < minZ)
                    minZ = tri.Vertex1.Z;
                #endregion

                #region Vert2
                if (tri.Vertex2.X > maxX)
                    maxX = tri.Vertex2.X;

                if (tri.Vertex2.Y > maxY)
                    maxY = tri.Vertex2.Y;

                if (tri.Vertex2.Z > maxZ)
                    maxZ = tri.Vertex2.Z;

                if (tri.Vertex2.X < minX)
                    minX = tri.Vertex2.X;

                if (tri.Vertex2.Y < minY)
                    minY = tri.Vertex2.Y;

                if (tri.Vertex2.Z < minZ)
                    minZ = tri.Vertex2.Z;
                #endregion

                #region Vert3
                if (tri.Vertex3.X > maxX)
                    maxX = tri.Vertex3.X;

                if (tri.Vertex3.Y > maxY)
                    maxY = tri.Vertex3.Y;

                if (tri.Vertex3.Z > maxZ)
                    maxZ = tri.Vertex3.Z;

                if (tri.Vertex3.X < minX)
                    minX = tri.Vertex3.X;

                if (tri.Vertex3.Y < minY)
                    minY = tri.Vertex3.Y;

                if (tri.Vertex3.Z < minZ)
                    minZ = tri.Vertex3.Z;
                #endregion
            }

            Max = new Vector3(maxX, maxY, maxZ);

            Min = new Vector3(minX, minY, minZ);

            Center.X = (Max.X + Min.X) / 2;

            Center.Y = (Max.Y + Min.Y) / 2;

            Center.Z = (Max.Z + Min.Z) / 2;

            Max -= Center;

            Min -= Center;

            Console.WriteLine("Min: (" + Min.X + ", " + Min.Y + ", " + Min.Z + ")");

            Console.WriteLine("Max: (" + Max.X + ", " + Max.Y + ", " + Max.Z + ")");

            Console.WriteLine("Center: (" + Center.X + ", " + Center.Y + ", " + Center.Z + ")");
        }
    }
}
