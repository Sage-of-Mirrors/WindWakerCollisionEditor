using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WindWakerCollisionEditor
{
    public enum TerrainType
    {
        Land,
        Water,
        Lava
    }

    public class Group : INotifyPropertyChanged, IRenderable
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private static int m_nameAddonInt;

        public string Name
        {
            get { return m_name; }
            set
            {
                if (value != m_name)
                {
                    m_name = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private string m_name;

        public BindingList<Triangle> Triangles
        {
            get { return m_triangles; }
            set
            {
                if (value != m_triangles)
                {
                    m_triangles = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private BindingList<Triangle> m_triangles;

        public Category GroupCategory
        {
            get { return m_groupCategory; }
            set
            {
                if (value != m_groupCategory)
                {
                    m_groupCategory = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Category m_groupCategory;

        public TerrainType Terrain
        {
            get { return m_terrain; }
            set
            {
                if (value != m_terrain)
                {
                    m_terrain = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private TerrainType m_terrain;

        private int m_roomNumber;

        public int RoomNumber
        {
            get { return  m_roomNumber; }
            set
            {
                if (value != m_roomNumber)
                { 
                    m_roomNumber = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Vector3 m_translation;

        private Vector3 m_rotation;

        private Vector3 m_scale;

        public int ParentIndex;

        private int m_vbo;

        private int m_notSelectEbo;

        private int m_selectEbo;

        private List<Vector3> m_vertexBuffer;

        private List<int> m_notObjBuffer;

        private List<int> m_selObjBuffer;

        public Group(EndianBinaryReader stream)
        {
            m_triangles = new BindingList<Triangle>();

            int streamStart = (int)stream.BaseStream.Position;

            stream.BaseStream.Position = stream.ReadInt32();

            char[] tempChars = Encoding.ASCII.GetChars(stream.ReadBytesUntil(0));

            m_name = new string(tempChars);

            stream.BaseStream.Position = streamStart + 4;

            Vector3 tempScale = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());

            m_scale = tempScale;

            m_rotation = new Vector3((float)stream.ReadUInt16(), (float)stream.ReadUInt16(), (float)stream.ReadUInt16());

            stream.Skip(2);

            Vector3 tempTrans = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());

            m_translation = tempTrans;

            ParentIndex = stream.ReadInt16();

            stream.Skip(12);

            m_terrain = (TerrainType)stream.ReadByte();

            m_roomNumber = stream.ReadByte();
        }

        public Group()
        {
            m_name = "NewGroup" + m_nameAddonInt++;

            m_triangles = new BindingList<Triangle>();

            m_terrain = TerrainType.Land;

            m_translation = new Vector3();

            m_rotation = new Vector3();

            m_scale = Vector3.One;

            m_selObjBuffer = new List<int>();

            m_notObjBuffer = new List<int>();
        }

        public void CreateBufferObjects()
        {
            m_vertexBuffer = new List<Vector3>();
            m_selObjBuffer = new List<int>();
            m_notObjBuffer = new List<int>();

            for (int i = 0; i < m_triangles.Count(); i++)
            {
                Vector3[] tempVec = new Vector3[] { m_triangles[i].Vertex1, m_triangles[i].Vertex2, m_triangles[i].Vertex3 };

                m_vertexBuffer.AddRange(tempVec);
            }

            //Generate a VBO, bind it, and upload the vertex array to it
            GL.GenBuffers(1, out m_vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_vertexBuffer.Count * Vector3.SizeInBytes),
                m_vertexBuffer.ToArray(), BufferUsageHint.StaticDraw);

            //Generate EBOs for unselected and selected triangles. The data will be uploaded to the GPU
            //when we draw the scene instead of right now.
            GL.GenBuffers(1, out m_notSelectEbo);

            GL.GenBuffers(1, out m_selectEbo);

            foreach (Triangle tri in Triangles)
            {
                int[] tempInt = new int[] { m_vertexBuffer.IndexOf(tri.Vertex1),
                    m_vertexBuffer.IndexOf(tri.Vertex2), m_vertexBuffer.IndexOf(tri.Vertex3) };

                tri.vertIndices = tempInt;
            }
        }

        public void Render(int _uniformMVP, int _uniformColor, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            //Clear the element buffers. If you don't, they just keep accumulating indexes forever!
            m_selObjBuffer.Clear();

            m_notObjBuffer.Clear();

            //Sorting through the triangles. The main idea here is to put triangles into one of two
            //meshes so that we can display one mesh as one color (black) and the other as another (red)
            foreach (Triangle tri in m_triangles)
            {
                if (tri.IsSelected)
                {
                    /*
                    int[] tempInt = new int[] { m_vertexBuffer.IndexOf(tri.Vertex1),
                    m_vertexBuffer.IndexOf(tri.Vertex2), m_vertexBuffer.IndexOf(tri.Vertex3) };

                    m_selObjBuffer.AddRange(tempInt);
                     * */

                    m_selObjBuffer.AddRange(tri.vertIndices);
                }

                else
                {
                    /*int[] tempInt = new int[] { m_vertexBuffer.IndexOf(tri.Vertex1),
                    m_vertexBuffer.IndexOf(tri.Vertex2), m_vertexBuffer.IndexOf(tri.Vertex3) };

                    m_notObjBuffer.AddRange(tempInt);*/

                    m_notObjBuffer.AddRange(tri.vertIndices);
                }
            }

            #region Binding vertex buffer

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);

            #endregion

            Matrix4 finalMatrix = Matrix4.Identity * viewMatrix * projMatrix;

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1.0f, 1.0f);

            #region Rendering unselected triangles (Fill)

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Set the color to black
            GL.Uniform4(_uniformColor, new Color4(Color4.PowderBlue.R, Color4.PowderBlue.G,
                Color4.PowderBlue.B, .65f));

            //Bind the unselected triangle index buffer and upload it to the GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_notSelectEbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_notObjBuffer.Count * 4),
                m_notObjBuffer.ToArray(), BufferUsageHint.StaticDraw);

            //Bind the VBO and the unselected EBO for drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_notSelectEbo);

            //Describe to the GPU how the data we just uploaded is laid out
            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false,
                Vector3.SizeInBytes, 0);

            //Upload the WVP to the GPU
            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            //Draw the data
            GL.DrawElements(BeginMode.Triangles, m_notObjBuffer.Count, DrawElementsType.UnsignedInt, 0);

            //Undo changes made to OpenGL context
            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);

            #endregion
            
            GL.Disable(EnableCap.PolygonOffsetFill);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Disable(EnableCap.Blend);

            #region Rendering unselected triangles (Lines)

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            //Set the color to black
            GL.Uniform4(_uniformColor, Color4.Black);

            //Bind the unselected triangle index buffer and upload it to the GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_notSelectEbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_notObjBuffer.Count * 4),
                m_notObjBuffer.ToArray(), BufferUsageHint.StaticDraw);

            //Bind the VBO and the unselected EBO for drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_notSelectEbo);

            //Describe to the GPU how the data we just uploaded is laid out
            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false,
                Vector3.SizeInBytes, 0);

            //Upload the WVP to the GPU
            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            //Draw the data
            GL.DrawElements(BeginMode.Triangles, m_notObjBuffer.Count, DrawElementsType.UnsignedInt, 0);

            //Undo changes made to OpenGL context
            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);

            #endregion

            GL.Disable(EnableCap.Texture2D);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1.0f, 1.0f);

            #region Rendering selected triangles (Fill)

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Uniform4(_uniformColor, new Color4(Color4.Red.R, Color4.Red.G,
                Color4.Red.B, .75f));

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_selectEbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_selObjBuffer.Count * 4),
                m_selObjBuffer.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_selectEbo);

            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false,
                Vector3.SizeInBytes, 0);

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.DrawElements(BeginMode.Triangles, m_selObjBuffer.Count(), DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);

            #endregion

            GL.Disable(EnableCap.PolygonOffsetFill);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Disable(EnableCap.Blend);

            #region Rendering selected triangles (Lines)

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.Uniform4(_uniformColor, Color4.Black);

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_selectEbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_selObjBuffer.Count * 4),
                m_selObjBuffer.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_selectEbo);

            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false,
                Vector3.SizeInBytes, 0);

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.DrawElements(BeginMode.Triangles, m_selObjBuffer.Count(), DrawElementsType.UnsignedInt, 0);

            #endregion

            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
        }
    }
}
