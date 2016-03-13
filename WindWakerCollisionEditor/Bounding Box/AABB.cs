using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WindWakerCollisionEditor
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

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;

            Max = max;

            Center.X = (Max.X + Min.X) / 2;

            Center.Y = (Max.Y + Min.Y) / 2;

            Center.Z = (Max.Z + Min.Z) / 2;

            //Min -= Center;

            //Max -= Center;
        }

        public AABB(List<Triangle> triList)
        {
            float maxX = float.MinValue;

            float maxY = float.MinValue;

            float maxZ = float.MinValue;

            float minX = float.MaxValue;

            float minY = float.MaxValue;

            float minZ = float.MaxValue;

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

            //Max -= Center;

            //Min -= Center;
        }

        public bool Contains(Vector3 point)
        {
            Vector3 realMin = Min; //+ Center;
            Vector3 realMax = Max; //+ Center;

            Vector3 realpoint = point - Center;

            //Console.WriteLine("Max:" + Max + ", Min:" + Min + ", Tri center:" + point);

            if ((point.X >= realMin.X) && (point.X <= realMax.X))
            {
                if ((point.Y >= realMin.Y) && (point.Y <= realMax.Y))
                {
                    if ((point.Z >= realMin.Z) && (point.Z <= realMax.Z))
                    {
                        //Console.WriteLine("passed");
                        return true;
                    }
                }
            }
            //Console.WriteLine("failed");
            return false;
        }

        public List<AABB> OctantSubdivide(AABB octantBoundingBox)
        {
            List<AABB> subdividedBox = new List<AABB>();

            Vector3 middleCoords = new Vector3((octantBoundingBox.Max.X + octantBoundingBox.Min.X) / 2,
                (octantBoundingBox.Max.Y + octantBoundingBox.Min.Y) / 2, (octantBoundingBox.Max.Z + octantBoundingBox.Min.Z) / 2);

            Vector3 oct2Max = new Vector3(middleCoords.X, middleCoords.Y, middleCoords.Z);

            Vector3 oct2Min = new Vector3(octantBoundingBox.Min.X, octantBoundingBox.Min.Y, octantBoundingBox.Min.Z);

            AABB oct2 = new AABB(oct2Min, oct2Max);

            subdividedBox.Add(oct2);

            Vector3 oct1Max = new Vector3(octantBoundingBox.Max.X, middleCoords.Y, middleCoords.Z);

            Vector3 oct1Min = new Vector3(middleCoords.X, octantBoundingBox.Min.Y, octantBoundingBox.Min.Z);

            AABB oct1 = new AABB(oct1Min, oct1Max);

            subdividedBox.Add(oct1);

            Vector3 oct6Max = new Vector3(middleCoords.X, octantBoundingBox.Max.Y, middleCoords.Z);

            Vector3 oct6Min = new Vector3(octantBoundingBox.Min.X, middleCoords.Y, octantBoundingBox.Min.Z);

            AABB oct6 = new AABB(oct6Min, oct6Max);

            subdividedBox.Add(oct6);

            Vector3 oct5Max = new Vector3(octantBoundingBox.Max.X, octantBoundingBox.Max.Y, middleCoords.Z);

            Vector3 oct5Min = new Vector3(middleCoords.X, middleCoords.Y, octantBoundingBox.Min.Z);

            AABB oct5 = new AABB(oct5Min, oct5Max);

            subdividedBox.Add(oct5);

            Vector3 oct8Max = new Vector3(middleCoords.X, octantBoundingBox.Max.Y, octantBoundingBox.Max.Z);

            Vector3 oct8Min = new Vector3(octantBoundingBox.Min.X, middleCoords.Y, middleCoords.Z);

            AABB oct8 = new AABB(oct8Min, oct8Max);

            subdividedBox.Add(oct8);

            Vector3 oct7Max = new Vector3(octantBoundingBox.Max.X, octantBoundingBox.Max.Y, octantBoundingBox.Max.Z);

            Vector3 oct7Min = new Vector3(middleCoords.X, middleCoords.Y, middleCoords.Z);

            AABB oct7 = new AABB(oct7Min, oct7Max);

            subdividedBox.Add(oct7);

            Vector3 oct4Max = new Vector3(middleCoords.X, middleCoords.Y, octantBoundingBox.Max.Z);

            Vector3 oct4Min = new Vector3(octantBoundingBox.Min.X, octantBoundingBox.Min.Y, middleCoords.Z);

            AABB oct4 = new AABB(oct4Min, oct4Max);

            subdividedBox.Add(oct4);

            Vector3 oct3Max = new Vector3(octantBoundingBox.Max.X, middleCoords.Y, octantBoundingBox.Max.Z);

            Vector3 oct3Min = new Vector3(middleCoords.X, octantBoundingBox.Min.Y, middleCoords.Z);

            AABB oct3 = new AABB(oct3Min, oct3Max);

            subdividedBox.Add(oct3);

            return subdividedBox;
        }
    }
}
