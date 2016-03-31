using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameFormatReader.Common;
using OpenTK;

namespace WindWakerCollisionEditor
{
    class DZB : IModelSource
    {
        List<Vector3> Vertexes;
        List<Triangle> Faces;
        List<Property> Properties;
        ObservableCollection<Category> Categories;

        int VertexCount;
        int FaceCount;
        int PropertyCount;
        int GroupCount;

        int VertexStartOffset;
        int FaceStartOffset;
        int PropertyStartOffset;
        int GroupStartOffset;

        public DZB()
        {
            Categories = new ObservableCollection<Category>();
        }

        // Keeping this for posterity until Load(string fileName) is proven to work
        public DZB(EndianBinaryReader reader)
        {
            VertexCount = reader.ReadInt32();
            VertexStartOffset = reader.ReadInt32();

            FaceCount = reader.ReadInt32();
            FaceStartOffset = reader.ReadInt32();

            reader.BaseStream.Position = 0x20;

            GroupCount = reader.ReadInt32();
            GroupStartOffset = 0x34 + reader.ReadInt32();

            PropertyCount = reader.ReadInt32();
            PropertyStartOffset = reader.ReadInt32();

            Vertexes = new List<Vector3>();

            reader.BaseStream.Position = 0x34;

            for (int i = 0; i < VertexCount; i++)
            {
                Vector3 tempVec = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                Vertexes.Add(tempVec);
            }

            Properties = new List<Property>();

            reader.BaseStream.Position = PropertyStartOffset;

            for (int i = 0; i < PropertyCount; i++)
            {
                Property tempProp = new Property(reader);

                Properties.Add(tempProp);
            }

            Faces = new List<Triangle>();

            reader.BaseStream.Position = FaceStartOffset;

            for (int i = 0; i < FaceCount; i++)
            {
                Triangle face = new Triangle(Vertexes[reader.ReadInt16()], Vertexes[reader.ReadInt16()],
                    Vertexes[reader.ReadInt16()], Properties[reader.ReadInt16()], reader.ReadInt16());

                Faces.Add(face);
            }

            reader.BaseStream.Position = GroupStartOffset;

            for (int i = 1; i < GroupCount; i++)
            {
                int groupStartPos = (int)reader.BaseStream.Position;

                reader.BaseStream.Position += 0x24;

                short parentIndex = reader.ReadInt16();

                reader.BaseStream.Position += 0x8;

                short octreeIndex = reader.ReadInt16();

                reader.BaseStream.Position = groupStartPos;

                //If octreeIndex is not -1, then that means this data 
                //is a group and has face data associated with it
                if (octreeIndex >= 0)
                {
                    Group geo = new Group(reader);

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

                            cat.Terrain = geo.Terrain;

                            cat.RoomNumber = geo.RoomNumber;

                            cat.Groups.Add(geo);
                        }
                    }
                }
                
                //If octreeIndex is -1, then we treat this data as a Category, with no
                //face data.
                else if (octreeIndex == -1)
                {
                    Category cat = new Category(reader);

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

        // Keeping this for posterity until Load(string fileName) is proven to work
        public ObservableCollection<Category> GetCategories()
        {
            return Categories;
        }

        public IEnumerable<Category> Load(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                VertexCount = reader.ReadInt32();
                VertexStartOffset = reader.ReadInt32();

                FaceCount = reader.ReadInt32();
                FaceStartOffset = reader.ReadInt32();

                reader.BaseStream.Position = 0x20;

                GroupCount = reader.ReadInt32();
                GroupStartOffset = 0x34 + reader.ReadInt32();

                PropertyCount = reader.ReadInt32();
                PropertyStartOffset = reader.ReadInt32();

                Vertexes = new List<Vector3>();

                reader.BaseStream.Position = 0x34;

                for (int i = 0; i < VertexCount; i++)
                {
                    Vector3 tempVec = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    Vertexes.Add(tempVec);
                }

                Properties = new List<Property>();

                reader.BaseStream.Position = PropertyStartOffset;

                for (int i = 0; i < PropertyCount; i++)
                {
                    Property tempProp = new Property(reader);

                    Properties.Add(tempProp);
                }

                Faces = new List<Triangle>();

                reader.BaseStream.Position = FaceStartOffset;

                for (int i = 0; i < FaceCount; i++)
                {
                    Triangle face = new Triangle(Vertexes[reader.ReadInt16()], Vertexes[reader.ReadInt16()],
                        Vertexes[reader.ReadInt16()], Properties[reader.ReadInt16()], reader.ReadInt16());

                    Faces.Add(face);
                }

                reader.BaseStream.Position = GroupStartOffset;

                for (int i = 1; i < GroupCount; i++)
                {
                    int groupStartPos = (int)reader.BaseStream.Position;

                    reader.BaseStream.Position += 0x24;

                    short parentIndex = reader.ReadInt16();

                    reader.BaseStream.Position += 0x8;

                    short octreeIndex = reader.ReadInt16();

                    reader.BaseStream.Position = groupStartPos;

                    //If octreeIndex is not -1, then that means this data 
                    //is a group and has face data associated with it
                    if (octreeIndex >= 0)
                    {
                        Group geo = new Group(reader);

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
                            if (cat.InitialIndex == parentIndex)
                            {
                                geo.GroupCategory = cat;

                                cat.Terrain = geo.Terrain;

                                cat.RoomNumber = geo.RoomNumber;

                                cat.Groups.Add(geo);
                            }
                        }
                    }

                    //If octreeIndex is -1, then we treat this data as a Category, with no
                    //face data.
                    else if (octreeIndex == -1)
                    {
                        Category cat = new Category(reader);

                        cat.InitialIndex = i;

                        Categories.Add(cat);
                    }
                }
            }

            return Categories;
        }
    }
}
