using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace CollisionEditor
{
    public class DZBExporter
    {
        List<Category> categoryList;

        // For the most part, these lists are for getting indexes.
        List<Hierarchy> hierarchy = new List<Hierarchy>();

        List<Vector3> vertexList = new List<Vector3>();

        List<Vector3> groupVertList = new List<Vector3>();

        List<Triangle> triList = new List<Triangle>();

        List<Octet> octreeList = new List<Octet>();

        List<Property> propList = new List<Property>();

        List<int> spatialListIndex = new List<int>();

        #region Data streams
        // These hold the individual sections of the file and are written to as the data is encountered.
        EndianBinaryWriter headerData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Header chunk

        EndianBinaryWriter vertexData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Vertex chunk

        EndianBinaryWriter faceData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Face chunk

        EndianBinaryWriter octreeData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Octree chunk

        EndianBinaryWriter octFaceIndexData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Octree Face Index Chunk

        EndianBinaryWriter propertyData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Property chunk

        EndianBinaryWriter hierarchyData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Group chunk

        EndianBinaryWriter hierNameData = new EndianBinaryWriter(new MemoryStream(), Endian.Big); // Group name chunk
        #endregion

        public DZBExporter(List<Category> catList, string fileName)
        {
            categoryList = catList;

            Hierarchy rootNode = new Hierarchy(fileName);

            // Add root node. All categories in the master Category list are implicitly children of this node.
            hierarchy.Add(rootNode);

            for (int i = 0; i < categoryList.Count; i++)
            {
                PrepareCategory(categoryList[i], i);
            }

            for (int i = 0; i < categoryList.Count; i++)
            {
                hierarchy[i + 1].FirstChildIndex = (short)hierarchy.Count;

                for (int j = 0; j < categoryList[i].Groups.Count; j++)
                {
                    PrepareGroup(categoryList[i].Groups[j], hierarchy[i + 1], j, i);
                }
            }

            foreach (Hierarchy hier in hierarchy)
                hier.Write(hierarchyData);

            foreach (Vector3 vec in vertexList)
                WriteVertexToStream(vec, vertexData);

            PrepareOctree();

            while ((octreeData.BaseStream.Length) % 4 != 0)
                octreeData.Write((byte)255);

            while ((octFaceIndexData.BaseStream.Length) % 4 != 0)
                octFaceIndexData.Write((byte)255);

            while ((faceData.BaseStream.Length) % 4 != 0)
                faceData.Write((byte)255);

            PrepareHeader();

            // This is the end of the file
            int nameTableOffset = (int)(headerData.BaseStream.Length + vertexData.BaseStream.Length + faceData.BaseStream.Length
                + octreeData.BaseStream.Length + octFaceIndexData.BaseStream.Length + propertyData.BaseStream.Length
                + hierarchyData.BaseStream.Length);

            hierarchyData.BaseStream.Position = 0;

            for (int i = 0; i < hierarchy.Count; i++)
            {
                hierarchyData.BaseStream.Position = 0x34 * i;

                hierarchyData.Write((int)(nameTableOffset + hierNameData.BaseStream.Length));

                hierNameData.Write(hierarchy[i].Name.ToCharArray());

                hierNameData.Write((byte)0);
            }
        }

        private void PrepareCategory(Category cat, int index)
        {
            Hierarchy categoryHier = new Hierarchy(cat);

            hierarchy.Add(categoryHier);

            if (index != categoryList.Count - 1)
            {
                categoryHier.NextSiblingIndex = (short)(hierarchy.Count);
            }
        }

        private void PrepareGroup(Group grp, Hierarchy parent, int index, int catIndex)
        {
            groupVertList = new List<Vector3>();

            Hierarchy groupHier = new Hierarchy(grp);

            hierarchy.Add(groupHier);

            groupHier.ParentIndex = (short)hierarchy.IndexOf(parent);

            groupHier.FirstVertexIndex = (short)vertexList.Count;

            if (index != categoryList[catIndex].Groups.Count - 1)
            {
                groupHier.NextSiblingIndex = (short)hierarchy.Count;
            }

            foreach (Triangle tri in grp.Triangles)
            {
                PrepareTriangle(tri, groupHier);
            }

            Octet oct = new Octet(new List<Triangle>(grp.Triangles));

            oct.Populate();

            PopulateOctreeList(oct);

            groupHier.OctreeStartIndex = (short)octreeList.IndexOf(oct);

            vertexList.AddRange(groupVertList);
        }

        private void PrepareTriangle(Triangle tri, Hierarchy groupHier)
        {
            if (groupVertList.Contains(tri.Vertex1))
            {
                faceData.Write((short)(groupVertList.IndexOf(tri.Vertex1) + vertexList.Count));
            }

            else
            {
                groupVertList.Add(tri.Vertex1);

                faceData.Write((short)(groupVertList.Count - 1 + vertexList.Count));
            }

            if (groupVertList.Contains(tri.Vertex2))
            {
                faceData.Write((short)(groupVertList.IndexOf(tri.Vertex2) + vertexList.Count));
            }

            else
            {
                groupVertList.Add(tri.Vertex2);

                faceData.Write((short)(groupVertList.Count - 1 + vertexList.Count));
            }

            if (groupVertList.Contains(tri.Vertex3))
            {
                faceData.Write((short)(groupVertList.IndexOf(tri.Vertex3) + vertexList.Count));
            }

            else
            {
                groupVertList.Add(tri.Vertex3);

                faceData.Write((short)(groupVertList.Count - 1 + vertexList.Count));
            }

            Property triProp = new Property(tri);

            short triPropIndex = (short)propList.IndexOf(triProp);

            if (triPropIndex < 0)
            {
                triPropIndex = (short)propList.Count;

                propList.Add(triProp);

                triProp.Write(propertyData);
            }

            // Write the indexes of the vertices for the face

            faceData.Write(triPropIndex);

            faceData.Write((short)(hierarchy.IndexOf(groupHier)));
        }

        private void PrepareOctree()
        {
            foreach (Octet oct in octreeList)
            {
                if (oct.IsLeafNode)
                {
                    octreeData.Write((short)257);

                    octreeData.Write((short)octreeList.IndexOf(oct.Parent));

                    octreeData.Write((short)(octFaceIndexData.BaseStream.Length / 2));

                    octFaceIndexData.Write((short)triList.IndexOf(oct.Triangles[0]));

                    spatialListIndex.Add(triList.IndexOf(oct.Triangles[0]));

                    // This fills the rest of the entry with padding
                    for (int i = 0; i < 14; i++)
                    {
                        octreeData.Write((byte)255);
                    }
                }

                else
                {
                    octreeData.Write((short)256);

                    octreeData.Write((short)octreeList.IndexOf(oct.Parent));

                    for (int i = 0; i < 8; i++)
                    {
                        // If the octant has no triangles, then just make its index -1
                        if (oct.Subdivisions[i] == null)
                            octreeData.Write((short)-1);
                        
                        else
                            octreeData.Write((short)octreeList.IndexOf(oct.Subdivisions[i]));
                    }
                }
            }
        }

        private void PopulateOctreeList(Octet oct)
        {
            if (oct == null)
                return;

            octreeList.Add(oct);

            if (oct.IsLeafNode)
            {
                triList.AddRange(oct.Triangles);

                //foreach (Triangle tri in oct.Triangles)
                    //PrepareTriangle(tri, null);

                return;
            }

            foreach (Octet octa in oct.Subdivisions)
                PopulateOctreeList(octa);
        }

        private void PrepareHeader()
        {
            int curOffset = 0x34;

            // Vertex count and offset to data
            headerData.Write(vertexList.Count);

            headerData.Write(curOffset);

            // Face count and offset to data
            headerData.Write((int)(faceData.BaseStream.Length / 0xA));

            curOffset = curOffset + (vertexList.Count * 0xC);

            headerData.Write(curOffset);

            // Octree face index count and offset to data
            headerData.Write(spatialListIndex.Count);

            curOffset = curOffset + (int)((faceData.BaseStream.Length) + (octreeData.BaseStream.Length) + (propertyData.BaseStream.Length));

            headerData.Write(curOffset);

            // Octree count and offset to data
            headerData.Write((int)(octreeData.BaseStream.Length / 0x14));

            curOffset = curOffset - (int)(octreeData.BaseStream.Length + propertyData.BaseStream.Length);

            headerData.Write(curOffset);

            // Hierarchy count and offset to data
            headerData.Write(hierarchy.Count);

            curOffset = curOffset + (int)((octreeData.BaseStream.Length) + (octFaceIndexData.BaseStream.Length) + (propertyData.BaseStream.Length));

            headerData.Write(curOffset);

            // Property count and offset to data
            headerData.Write(propList.Count);

            curOffset = curOffset - (int)(propertyData.BaseStream.Length + octFaceIndexData.BaseStream.Length);

            headerData.Write(curOffset);

            // Padding
            headerData.Write((int)0);
        }

        private void WriteVertexToStream(Vector3 vertex, EndianBinaryWriter writer)
        {
            writer.Write(vertex.X);

            writer.Write(vertex.Y);

            writer.Write(vertex.Z);
        }

        public void Export(EndianBinaryWriter fileStream)
        {
            headerData.BaseStream.Position = 0;

            vertexData.BaseStream.Position = 0;

            faceData.BaseStream.Position = 0;

            octreeData.BaseStream.Position = 0;

            octFaceIndexData.BaseStream.Position = 0;

            propertyData.BaseStream.Position = 0;

            hierarchyData.BaseStream.Position = 0;

            hierNameData.BaseStream.Position = 0;

            headerData.BaseStream.CopyTo(fileStream.BaseStream);

            vertexData.BaseStream.CopyTo(fileStream.BaseStream);

            faceData.BaseStream.CopyTo(fileStream.BaseStream);

            octreeData.BaseStream.CopyTo(fileStream.BaseStream);

            propertyData.BaseStream.CopyTo(fileStream.BaseStream);

            octFaceIndexData.BaseStream.CopyTo(fileStream.BaseStream);

            hierarchyData.BaseStream.CopyTo(fileStream.BaseStream);

            hierNameData.BaseStream.CopyTo(fileStream.BaseStream);

            while (fileStream.BaseStream.Length % 4 != 0)
                fileStream.Write((byte)0xFF);

            fileStream.Close();
        }
    }
}
