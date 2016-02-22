using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    class DZB
    {
        List<Vector3> Vertexes;
        List<Triangle> Faces;
        List<Property> Properties;
        List<Category> Categories;

        int VertexCount;
        int FaceCount;
        int PropertyCount;
        int GroupCount;

        int VertexStartOffset;
        int FaceStartOffset;
        int PropertyStartOffset;
        int GroupStartOffset;

        public DZB(EndianBinaryReader stream)
        {
            VertexCount = stream.ReadInt32();
            VertexStartOffset = stream.ReadInt32();

            FaceCount = stream.ReadInt32();
            FaceStartOffset = stream.ReadInt32();

            stream.BaseStream.Position = 0x20;

            GroupCount = stream.ReadInt32();
            GroupStartOffset = 0x34 + stream.ReadInt32();

            PropertyCount = stream.ReadInt32();
            PropertyStartOffset = stream.ReadInt32();

            Vertexes = new List<Vector3>();

            stream.BaseStream.Position = 0x34;

            for (int i = 0; i < VertexCount; i++)
            {
                Vector3 tempVec = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());

                Vertexes.Add(tempVec);
            }

            Properties = new List<Property>();

            stream.BaseStream.Position = PropertyStartOffset;

            for (int i = 0; i < PropertyCount; i++)
            {
                Property tempProp = new Property();

                Properties.Add(tempProp);
            }

            Faces = new List<Triangle>();

            stream.BaseStream.Position = FaceStartOffset;

            for (int i = 0; i < FaceCount; i++)
            {
                Triangle face = new Triangle(Vertexes[stream.ReadInt16()], Vertexes[stream.ReadInt16()],
                    Vertexes[stream.ReadInt16()], Properties[stream.ReadInt16()], stream.ReadInt16());

                Faces.Add(face);
            }

            stream.BaseStream.Position = GroupStartOffset;

            Categories = new List<Category>();

            for (int i = 1; i < GroupCount; i++)
            {
                int groupStartPos = (int)stream.BaseStream.Position;

                stream.BaseStream.Position += 0x24;

                short parentIndex = stream.ReadInt16();

                stream.BaseStream.Position += 0x8;

                short octreeIndex = stream.ReadInt16();

                stream.BaseStream.Position = groupStartPos;

                //If octreeIndex is not -1, then that means this data 
                //is a group and has face data associated with it
                if (octreeIndex >= 0)
                {
                    Group geo = new Group(stream);

                    foreach (Triangle face in Faces)
                    {
                        if (face.GroupIndex == i)
                        {
                            face.ParentGroup = geo;

                            geo.Triangles.Add(face);
                        }
                    }

                    geo.CreateBufferObjects();

                    foreach (Category cat in Categories)
                    {
                        if (cat.InitialIndex == parentIndex )
                        {
                            geo.GroupCategory = cat;

                            cat.Groups.Add(geo);
                        }
                    }
                }
                
                //If octreeIndex is -1, then we treat this data as a Category, with no
                //face data.
                else if (octreeIndex == -1)
                {
                    Category cat = new Category(stream);

                    cat.InitialIndex = i;

                    Categories.Add(cat);

                        /* Pending analysis beacuse wtf
                    else
                    {
                        GeoGroup geo = new GeoGroup(stream);

                        foreach (EditorFace face in Faces)
                        {
                            if (face.GroupIndex == i)
                            {
                                geo.Faces.Add(face);
                            }
                        }

                        ParentGroup tempheh = Root.ChildGroups[geo.ParentIndex - 1] as ParentGroup;

                        tempheh.ChildGroups.Add(geo);
                    }
                         * */
                }
            }
        }

        public ObservableCollection<Category> GetCategories()
        {
            return new ObservableCollection<Category>(Categories);
        }
    }
}
