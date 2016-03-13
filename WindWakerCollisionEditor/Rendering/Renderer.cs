using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows;
using System.Windows.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WindWakerCollisionEditor
{
    public enum ShaderAttributeIds
    {
        Position, Color,
        TexCoord, Normal
    }

    class Renderer : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Camera Cam
        {
            get { return m_Camera; }
            set
            {
                if (value != m_Camera)
                {
                    m_Camera = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Camera m_Camera;

        public List<IRenderable> RenderableObjs { get { return m_renderableObjs; } set { m_renderableObjs = value; } }
        private List<IRenderable> m_renderableObjs;
        
        private System.Windows.Forms.Timer m_intervalTimer;

        public GLControl m_control;

        private int _programID;
        private int _uniformMVP;
        private int _uniformColor;

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        private Color4 debugRayColor = Color4.Yellow;

        public event EventHandler<SelectTriangleEventArgs> SelectedTris;

        SelectTriangleEventArgs args = new SelectTriangleEventArgs();

        public event EventHandler RegroupTris;

        public event EventHandler RecategorizeTris;

        public event EventHandler FocusCamera;

        #region Construction

        public Renderer(GLControl context, WindowsFormsHost host)
        {
            m_control = context;

            Cam = new Camera();

            SetUpViewport();

            m_intervalTimer = new System.Windows.Forms.Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            {
                Vector2 mousePosGlobal = new Vector2(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
                Vector2 glControlPosGlobal = new Vector2((float)host.PointToScreen(new Point(0, 0)).X, (float)host.PointToScreen(new Point(0, 0)).Y);

                Input.Internal_SetMousePos(new Vector2(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y));

                Cam.Update();

                Input.Internal_UpdateInputState();

                Draw();
            };

            m_renderableObjs = new List<IRenderable>();

            m_control.MouseUp += m_control_MouseUp;

            m_control.MouseDown += m_control_MouseDown;

            m_control.MouseMove += m_control_MouseMove;

            m_control.MouseWheel += m_control_MouseWheel;

            host.KeyUp += host_KeyUp;

            host.KeyDown += host_KeyDown;

            host.LayoutUpdated += host_LayoutUpdated;

            host.MouseWheel += host_MouseWheel;
        }

        void m_control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.SetMouseScrollDelta(e.Delta);
        }

        void host_MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        void m_control_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void SetUpViewport()
        {
            _programID = GL.CreateProgram();

            Cam = new Camera();

            int vertShaderId, fragShaderId;
            LoadShader(@"Rendering/vs.glsl", ShaderType.VertexShader, _programID, out vertShaderId);
            LoadShader(@"Rendering/fs.glsl", ShaderType.FragmentShader, _programID, out fragShaderId);

            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(_programID, (int)ShaderAttributeIds.Position, "vertexPos");

            GL.LinkProgram(_programID);

            _uniformMVP = GL.GetUniformLocation(_programID, "modelview");
            _uniformColor = GL.GetUniformLocation(_programID, "col");

            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(_programID));
        }

        private void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (var streamReader = new StreamReader(fileName))
            {
                GL.ShaderSource(address, streamReader.ReadToEnd());
            }

            GL.CompileShader(address);

            GL.AttachShader(program, address);

            int compileSuccess;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compileSuccess);

            if (compileSuccess == 0)
                Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public Camera GetCamera()
        {
            return Cam;
        }

        #endregion

        #region Rendering

        private void Draw()
        {
            GL.ClearColor(new Color4(.36f, .25f, .94f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_programID);

            GL.Enable(EnableCap.DepthTest);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            float width, height;

            if (m_control.Width == 0)
                width = 1f;

            else
                width = m_control.Width;

            if (m_control.Height == 0)
                height = 1f;

            else
                height = m_control.Height;

            ViewMatrix = Cam.ViewMatrix;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), width / height, 100, 500000);

            //Render stuff goes here

            foreach (IRenderable rend in m_renderableObjs)
                rend.Render(_uniformMVP, _uniformColor, ViewMatrix, ProjectionMatrix);

            //RenderDebugTri();

            m_control.SwapBuffers();
        }

        private void RenderDebugTri()
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.Rotate(Quaternion.Identity) * Matrix4.Scale(1);

            Matrix4 finalMatrix = modelMatrix * ViewMatrix * ProjectionMatrix;

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.Uniform4(_uniformColor, debugRayColor);

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(0, 200, 0);
            GL.Vertex3(200, 0, 0);
            GL.Vertex3(-200, 0, 0);

            GL.End();
        }

        private void RenderDebugCube()
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.Rotate(Quaternion.Identity) * Matrix4.Scale(1);

            Matrix4 finalMatrix = modelMatrix * ViewMatrix * ProjectionMatrix;

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.Uniform4(_uniformColor, debugRayColor);

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);

            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, -25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(25f, -25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(25f, 25f, -25f);

            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);
            GL.Vertex3(-25f, -25f, 25f);

            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, 25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, -25f, 25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(-25f, -25f, 25f);

            /*
            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);
            */

            GL.End();
        }

        #endregion

        #region Events

        void m_control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, false);
        }

        void m_control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, true);

            args.SelectedTris.Clear();

            if (Input.GetMouseButton(0))
            {
                DoRayCast(e.X, e.Y);
            }
        }

        private void DoRayCast(int mousePosX, int mousePosY)
        {
            List<Group> groupList = new List<Group>();

            foreach (IRenderable rend in m_renderableObjs)
            {
                if (rend.GetType() == typeof(Group))
                    groupList.Add(rend as Group);
            }

                Triangle tri = Cam.CastAgainstTriangle(mousePosX, mousePosY,
                    m_control.Width, m_control.Height, ProjectionMatrix, groupList);

                if (tri != null)
                {
                    if (Input.GetKey(Keys.LControlKey))
                    {
                        if (!args.SelectedTris.Contains(tri))
                            args.SelectedTris.Add(tri);
                    }

                    else
                    {
                        foreach (Triangle tria in args.SelectedTris)
                            tria.IsSelected = false;

                        args.SelectedTris.Clear();

                        args.SelectedTris.Add(tri);
                    }
                }

            OnSelectObject(args);
        }

        protected virtual void OnSelectObject(SelectTriangleEventArgs e)
        {
            EventHandler<SelectTriangleEventArgs> handler = SelectedTris;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        void m_control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_control.Focus();
        }

        void host_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Input.Internal_SetKeyState((Keys)KeyInterop.VirtualKeyFromKey(e.Key), false);
        }

        void host_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Input.Internal_SetKeyState((Keys)KeyInterop.VirtualKeyFromKey(e.Key), true);

            if (Input.GetKey(Keys.LControlKey))
            {
                if (Input.GetKeyDown(Keys.G))
                {
                    RegroupTris(this, new EventArgs());
                }

                if (Input.GetKeyDown(Keys.Q))
                {
                    RecategorizeTris(this, new EventArgs());
                }
            }

            if (Input.GetKeyDown(Keys.F))
            {
                FocusCamera(this, new EventArgs());
            }

            if (Input.GetKey(Keys.OemMinus))
            {
                if (Cam.MoveSpeed > 100)
                    Cam.MoveSpeed -= 100;
            }


            if (Input.GetKey(Keys.Oemplus))
                Cam.MoveSpeed += 100;
        }

        void host_LayoutUpdated(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, m_control.Width, m_control.Height);
        }

        #endregion
    }

    class SelectTriangleEventArgs : EventArgs
    {
        public SelectTriangleEventArgs()
        {
            SelectedTris = new List<Triangle>();
        }

        public List<Triangle> SelectedTris { get; set; }
    }
}
