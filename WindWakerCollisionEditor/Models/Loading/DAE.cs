using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using grendgine_collada;
using OpenTK;

namespace WindWakerCollisionEditor
{
    class DAE : IModelSource
    {
        ObservableCollection<Category> categoryCollection;

        public DAE()
        {
            categoryCollection = new ObservableCollection<Category>();
        }

        // Keeping this for posterity until Load(string fileName) is proven to work
        public DAE(string fileName)
        {
            categoryCollection = new ObservableCollection<Category>();

            Grendgine_Collada file = Grendgine_Collada.Grendgine_Load_File(fileName);

            for (int i = 0; i < file.Library_Geometries.Geometry.Length; i++)
            {
                Category cat = new Category();

                cat.Name = file.Library_Geometries.Geometry[i].Name + "_parent";

                List<Vector3> verts = GetVerts(file.Library_Geometries.Geometry[i]);

                int skipValue = CalcSkipValue(file.Library_Geometries.Geometry[i]);

                Group grp = new Group();

                grp.Name = file.Library_Geometries.Geometry[i].Name;

                int[] pList = file.Library_Geometries.Geometry[i].Mesh.Polylist[0].P.Value();

                List<int> vertIndexes = GetVertIndexes(pList, skipValue);

                for (int j = 0; j < vertIndexes.Count; j += 3)
                {
                    Triangle face = new Triangle();

                    face.Vertex1 = verts[vertIndexes[j]];

                    face.Vertex2 = verts[vertIndexes[j + 1]];

                    face.Vertex3 = verts[vertIndexes[j + 2]];

                    face.ParentGroup = grp;

                    grp.Triangles.Add(face);
                }

                grp.CreateBufferObjects();

                grp.GroupCategory = cat;

                cat.Groups.Add(grp);

                categoryCollection.Add(cat);
            }
        }

        private List<Vector3> GetVerts(Grendgine_Collada_Geometry source)
        {
            // Horrible sanitization of 3dsmax's collada data
            string posFloatArray = source.Mesh.Source[0].Float_Array.Value_As_String;
            posFloatArray = posFloatArray.Replace('\n', ' ').Trim();
            source.Mesh.Source[0].Float_Array.Value_As_String = posFloatArray;

            float[] positions = source.Mesh.Source[0].Float_Array.Value();

            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < positions.Length; i += 3)
            {
                Vector3 tempVec = new Vector3(positions[i], positions[i + 1], positions[i + 2]);

                //Blender's axes are different from what they are in OpenGL. This is a hardcoded fix for that, but it will probably have
                //to get changed since the user might want to use a model exported from a program with the correct axes.aaxxa
                float y = tempVec.Y;

                float z = tempVec.Z;

                tempVec.Y = z;

                tempVec.Z = -y;

                verts.Add(tempVec);
            }

            return verts;
        }

        private int CalcSkipValue(Grendgine_Collada_Geometry source)
        {
            int skipVal = 0;

            if (source.Mesh.Polylist == null)
            {
                for (int i = 0; i < source.Mesh.Source.Length; i++)
                {
                    skipVal += 1;
                }
            }

            else
            {
                for (int i = 0; i < source.Mesh.Polylist[0].Input.Length; i++)
                {
                    switch (source.Mesh.Polylist[0].Input[i].Semantic)
                    {
                        case Grendgine_Collada_Input_Semantic.VERTEX:
                            skipVal += 1;
                            break;
                        case Grendgine_Collada_Input_Semantic.NORMAL:
                            skipVal += 1;
                            break;
                        case Grendgine_Collada_Input_Semantic.TEXCOORD:
                            skipVal += 1;
                            break;
                    }
                }
            }

            return skipVal;
        }

        private List<int> GetVertIndexes(int[] pList, int skip)
        {
            List<int> indexes = new List<int>();

            for (int i = 0; i < pList.Length / skip; i++)
            {
                indexes.Add(pList[i * skip]);
            }

            return indexes;
        }

        // Keeping this for posterity until Load(string fileName) is proven to work
        public ObservableCollection<Category> GetCategories()
        {
            return categoryCollection;
        }

        public IEnumerable<Category> Load(string fileName)
        {
            categoryCollection = new ObservableCollection<Category>();

            Grendgine_Collada file = Grendgine_Collada.Grendgine_Load_File(fileName);

            for (int i = 0; i < file.Library_Geometries.Geometry.Length; i++)
            {
                Category cat = new Category();

                cat.Name = file.Library_Geometries.Geometry[i].Name + "_parent";

                List<Vector3> verts = GetVerts(file.Library_Geometries.Geometry[i]);

                int skipValue = CalcSkipValue(file.Library_Geometries.Geometry[i]);

                Group grp = new Group();

                grp.Name = file.Library_Geometries.Geometry[i].Name;

                int[] pList = null;

                if (file.Library_Geometries.Geometry[i].Mesh.Polylist == null)
                {
                    // Horrible sanitization of 3dsmax's collada data
                    string plist = file.Library_Geometries.Geometry[i].Mesh.Triangles[0].P.Value_As_String;
                    plist = plist.Replace('\n', ' ').Trim();
                    file.Library_Geometries.Geometry[i].Mesh.Triangles[0].P.Value_As_String = plist;

                    pList = file.Library_Geometries.Geometry[i].Mesh.Triangles[0].P.Value();
                }

                else
                {
                    pList = file.Library_Geometries.Geometry[i].Mesh.Polylist[0].P.Value();
                }

                List<int> vertIndexes = GetVertIndexes(pList, skipValue);

                for (int j = 0; j < vertIndexes.Count; j += 3)
                {
                    Triangle face = new Triangle();

                    face.Vertex1 = verts[vertIndexes[j]];

                    face.Vertex2 = verts[vertIndexes[j + 1]];

                    face.Vertex3 = verts[vertIndexes[j + 2]];

                    face.ParentGroup = grp;

                    grp.Triangles.Add(face);
                }

                grp.CreateBufferObjects();

                grp.GroupCategory = cat;

                cat.Groups.Add(grp);

                categoryCollection.Add(cat);
            }

            return categoryCollection;
        }
    }
}
