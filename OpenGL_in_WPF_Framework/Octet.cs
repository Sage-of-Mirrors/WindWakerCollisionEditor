using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    class Octet
    {
        public bool IsLeafNode { get; set; }

        public Octet[] Subdivisions { get; set; }

        public List<Triangle> Triangles { get; set; }

        public Octet Parent { get; set; }

        AABB BoundingBox { get; set; }

        public Octet(List<Triangle> tris)
        {
            Triangles = tris;

            BoundingBox = new AABB(tris);

            Subdivisions = new Octet[8];
        }

        public Octet(AABB boundingBox)
        {
            Triangles = new List<Triangle>();

            BoundingBox = boundingBox;

            Subdivisions = new Octet[8];
        }

        public void Populate()
        {
            if (Triangles.Count <= 12)
            {
                IsLeafNode = true;

                return;
            }

            Vector3 dimensions = BoundingBox.Max - BoundingBox.Min;
            
            Vector3 half = dimensions / 2.0f;

            Vector3 center = BoundingBox.Min + half;

            AABB[] octants = new AABB[8];

            octants[0] = new AABB(BoundingBox.Min, center);
            octants[1] = new AABB(new Vector3(center.X, BoundingBox.Min.Y, BoundingBox.Min.Z), new Vector3(BoundingBox.Max.X, center.Y, center.Z));
            octants[2] = new AABB(new Vector3(BoundingBox.Min.X, center.Y, BoundingBox.Min.Z), new Vector3(center.X, BoundingBox.Max.Y, center.Z)); 
            octants[3] = new AABB(new Vector3(center.X, center.Y, BoundingBox.Min.Z), new Vector3(BoundingBox.Max.X, BoundingBox.Max.Y, center.Z));
            octants[4] = new AABB(new Vector3(BoundingBox.Min.X, center.Y, center.Z), new Vector3(center.X, BoundingBox.Max.Y, BoundingBox.Max.Z));
            octants[5] = new AABB(center, BoundingBox.Max);
            octants[6] = new AABB(new Vector3(BoundingBox.Min.X, BoundingBox.Min.Y, center.Z), new Vector3(center.X, center.Y, BoundingBox.Max.Z));
            octants[7] = new AABB(new Vector3(center.X, BoundingBox.Min.Y, center.Z), new Vector3(BoundingBox.Max.X, center.Y, BoundingBox.Max.Z));

            List<Triangle>[] octList = new List<Triangle>[8];

            for (int i = 0; i < 8; i++) octList[i] = new List<Triangle>();

            List<Triangle> deList = new List<Triangle>();

            foreach (Triangle tri in Triangles)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (octants[i].Contains(tri.GetCenter()))
                    {
                        octList[i].Add(tri);
                        deList.Add(tri);
                        break;
                    }
                }
            }

            foreach (Triangle tri in deList)
                Triangles.Remove(tri);

            for (int i = 0; i < 8; i++)
            {
                if (octList[i].Count != 0)
                {
                    Subdivisions[i] = CreateNode(octants[i], octList[i]);

                    Subdivisions[i].Populate();
                }
            }
        }

        private Octet CreateNode(AABB boundingBox, List<Triangle> triList)
        {
            if (triList.Count == 0)
                return null;

            Octet ret = new Octet(boundingBox);
            ret.Triangles = triList;
            ret.Parent = this;

            return ret;
        }
    }
}
