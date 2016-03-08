using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    class Hierarchy
    {
        public string Name { get; set; }

        public int NameIndex { get; set; }

        public Vector3 Scale { get; set; }

        public int[] Rotation { get; set; }

        public Vector3 Translation { get; set; }

        public short ParentIndex { get; set; }

        public short NextSiblingIndex { get; set; }

        public short FirstChildIndex { get; set; }

        public short Unknown1 { get; set; }

        public short FirstVertexIndex { get; set; }

        public short OctreeStartIndex { get; set; }

        public short Unknown3 { get; set; }

        public byte TerrainType { get; set; }

        public byte RoomNumber { get; set; }

        /// <summary>
        /// Creates the root node of the hierarchy.
        /// </summary>
        /// <param name="name">The name of the root node</param>
        public Hierarchy(string name)
        {
            Name = name;

            Scale = Vector3.One;

            Rotation = new int[] { 0, 0, 0 };

            Translation = Vector3.Zero;

            ParentIndex = -1;

            NextSiblingIndex = -1;

            FirstChildIndex = 1;

            Unknown1 = 0;

            FirstVertexIndex = 0;

            OctreeStartIndex = -1;

            Unknown3 = 0;

            TerrainType = 0;

            RoomNumber = 0;
        }

        /// <summary>
        /// Creates a node in the hierarchy from a Category.
        /// </summary>
        /// <param name="cat">The Category class to derive information from</param>
        public Hierarchy(Category cat)
        {
            Name = cat.Name;

            Scale = Vector3.One;

            Rotation = new int[] { 0, 0, 0 };

            Translation = Vector3.Zero;

            ParentIndex = 0;

            NextSiblingIndex = -1;

            FirstChildIndex = 1;

            Unknown1 = -1;

            FirstVertexIndex = 0;

            OctreeStartIndex = -1;

            Unknown3 = 0;

            TerrainType = 0;

            RoomNumber = 0;
        }

        /// <summary>
        /// Creates a node in the hierarchy from a Group.
        /// </summary>
        /// <param name="grp">The Group class to derive information from</param>
        public Hierarchy(Group grp)
        {
            Name = grp.Name;

            Scale = Vector3.One;

            Rotation = new int[] { 0, 0, 0 };

            Translation = Vector3.Zero;

            ParentIndex = -1;

            NextSiblingIndex = -1;

            FirstChildIndex = -1;

            Unknown1 = -1;

            FirstVertexIndex = 0;

            OctreeStartIndex = -1;

            Unknown3 = 0;

            TerrainType = (byte)grp.Terrain;

            RoomNumber = (byte)grp.RoomNumber;
        }

        /// <summary>
        /// Writes the property data to the specified stream
        /// </summary>
        /// <param name="writer">Stream to write data to</param>
        public void Write(EndianBinaryWriter writer)
        {
            // Placeholder for the offset to name
            writer.Write((int)0);


            writer.Write(Scale.X);

            writer.Write(Scale.Y);

            writer.Write(Scale.Z);


            writer.Write((short)Rotation[0]);

            writer.Write((short)Rotation[1]);

            writer.Write((short)Rotation[2]);

            writer.Write((short)-1);


            writer.Write(Translation.X);

            writer.Write(Translation.Y);

            writer.Write(Translation.Z);


            writer.Write(ParentIndex);

            writer.Write(NextSiblingIndex);

            writer.Write(FirstChildIndex);

            writer.Write(Unknown1);


            writer.Write(FirstVertexIndex);

            writer.Write(OctreeStartIndex);

            writer.Write(Unknown3);


            writer.Write(TerrainType);

            writer.Write(RoomNumber);
        }
    }
}
