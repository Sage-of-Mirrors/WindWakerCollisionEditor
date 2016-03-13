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

        #region Properties
        /// <summary>
        /// Represents the GLControl associated with this Renderer.
        /// </summary>
        public GLControl m_control;

        /// <summary>
        /// Regulates rendering calls to roughly 60 fps.
        /// </summary>
        private System.Windows.Forms.Timer m_intervalTimer;

        /// <summary>
        /// Args for the Triangle selection events
        /// </summary>
        SelectTriangleEventArgs args = new SelectTriangleEventArgs();

        /// <summary>
        /// Specifies the current selection type.
        /// </summary>
        public TriSelectionType SelectionType;

        #region Camera
        /// <summary>
        /// Represents the viewport's Camera.
        /// </summary>
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
        #endregion

        #region Debug
        /// <summary>
        /// Determines the color of an object when hit by a ray.
        /// Used for debugging the raycasting code.
        /// </summary>
        private Color4 debugRayColor = Color4.Yellow;
        #endregion

        #region Events
        public event EventHandler<SelectTriangleEventArgs> SelectedTris;

        public event EventHandler RegroupTris;

        public event EventHandler RecategorizeTris;

        public event EventHandler FocusCamera;
        #endregion

        #region Integers
        private int _programID;
        private int _uniformMVP;
        private int _uniformColor;
        #endregion

        #region Matrices
        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;
        #endregion

        #region Renderable Object List
        public List<IRenderable> RenderableObjs { get { return m_renderableObjs; } set { m_renderableObjs = value; } }
        private List<IRenderable> m_renderableObjs;
        #endregion
        #endregion

        #region Construction
        /// <summary>
        /// Sets up the Renderer and rendering methods.
        /// </summary>
        /// <param name="context">GLControl to associate with this Renderer</param>
        /// <param name="host">WindowsFormHost of the GLControl</param>
        public Renderer(GLControl context, WindowsFormsHost host)
        {
            m_control = context;

            Cam = new Camera();

            SetUpViewport();

            // Set up the timer for rendering
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

            m_control.MouseWheel += (args, o) => { Input.SetMouseScrollDelta(o.Delta); };

            host.KeyUp += host_KeyUp;

            host.KeyDown += host_KeyDown;

            host.LayoutUpdated += host_LayoutUpdated;
        }

        /// <summary>
        /// Sets up the viewport by creating a Camera and loading shaders.
        /// </summary>
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

        /// <summary>
        /// Loads a shader from file.
        /// </summary>
        /// <param name="fileName">Filename of the shader</param>
        /// <param name="type">ShaderType of the shader</param>
        /// <param name="program">OpenGL Program that the shader will be attatched to</param>
        /// <param name="address">Address of the shader once it has been loaded</param>
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
        #endregion

        #region Rendering
        /// <summary>
        /// Draws the scene to the viewport
        /// </summary>
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

            m_control.SwapBuffers();
        }

        /// <summary>
        /// Renders a triangle to the viewport.
        /// Used for debugging the viewport.
        /// </summary>
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

        /// <summary>
        /// Renders a cube to the viewport.
        /// Used for debugging the viewport.
        /// </summary>
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
        /// <summary>
        /// Handler for m_control's MouseUp event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, false);
        }

        /// <summary>
        /// Handle for m_control's MouseDown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, true);

            args.SelectedTris.Clear();

            if (Input.GetMouseButton(0))
            {
                DoRayCast(e.X, e.Y);
            }
        }

        /// <summary>
        /// Casts a ray using the specified X and Y coordinates from the mouse.
        /// </summary>
        /// <param name="mousePosX">The mouse's X position</param>
        /// <param name="mousePosY">The mouse's Y position</param>
        private void DoRayCast(int mousePosX, int mousePosY)
        {
            List<Group> groupList = new List<Group>();

            // Get a list of all the groups currently loaded
            foreach (IRenderable rend in m_renderableObjs)
            {
                if (rend.GetType() == typeof(Group))
                    groupList.Add(rend as Group);
            }

            // See if we hit a triangle
            Triangle tri = Cam.CastAgainstTriangle(mousePosX, mousePosY,
                m_control.Width, m_control.Height, ProjectionMatrix, groupList);

            // We hit a triangle
            if (tri != null)
            {
                // We're just going to select the individual triangles
                if (SelectionType == TriSelectionType.Triangle)
                {
                    // We're holding the left Control key, meaning we want to add the triangle to the current selection
                    if (Input.GetKey(Keys.LControlKey))
                    {
                        // Just checking to see if the triangle is already selected, so we don't add it to the selection twice
                        if (!args.SelectedTris.Contains(tri))
                            args.SelectedTris.Add(tri);
                    }

                    // Clear the current selection, then add the triangle to it
                    else
                    {
                        foreach (Triangle tria in args.SelectedTris)
                            tria.IsSelected = false;

                        args.SelectedTris.Clear();

                        args.SelectedTris.Add(tri);
                    }
                }

                // We want to select the group that this triangle belongs to.
                else if (SelectionType == TriSelectionType.Group)
                {
                    // We're holding the left Control key, meaning we want to add to the current selection
                    if (Input.GetKey(Keys.LControlKey))
                    {
                        foreach (Triangle tria in tri.ParentGroup.Triangles)
                        {
                            if (!args.SelectedTris.Contains(tria))
                                args.SelectedTris.Add(tria);
                        }
                    }

                    // Clear the current selection, then add the group's triangles to it
                    else
                    {
                        foreach (Triangle tria in args.SelectedTris)
                            tria.IsSelected = false;

                        args.SelectedTris.Clear();

                        foreach (Triangle tria in tri.ParentGroup.Triangles)
                        {
                            args.SelectedTris.Add(tria);
                        }
                    }
                }

                // We'll select all of the groups that are in the same category as the triangle's group.
                else if (SelectionType == TriSelectionType.Category)
                {
                    // We're holding the left Control key, meaning we want to add to the current selection
                    if (Input.GetKey(Keys.LControlKey))
                    {
                        foreach (Group gro in tri.ParentGroup.GroupCategory.Groups)
                        {
                            foreach (Triangle tria in gro.Triangles)
                            {
                                if (!args.SelectedTris.Contains(tria))
                                    args.SelectedTris.Add(tria);
                            }
                        }
                    }

                    // Clear the current selection, then add the group's triangles to it
                    else
                    {
                        foreach (Triangle tria in args.SelectedTris)
                            tria.IsSelected = false;

                        args.SelectedTris.Clear();

                        foreach (Group gro in tri.ParentGroup.GroupCategory.Groups)
                        {
                            foreach (Triangle tria in gro.Triangles)
                            {
                                args.SelectedTris.Add(tria);
                            }
                        }
                    }
                }
            }

            OnSelectObject(args);
        }

        /// <summary>
        /// Handles Triangle selection.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectObject(SelectTriangleEventArgs e)
        {
            EventHandler<SelectTriangleEventArgs> handler = SelectedTris;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Handler for m_control's MouseMove event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_control.Focus();
        }

        /// <summary>
        /// Handler for host's KeyUp event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void host_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Input.Internal_SetKeyState((Keys)KeyInterop.VirtualKeyFromKey(e.Key), false);
        }

        /// <summary>
        /// Handler for host's KeyDown event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (Input.GetKeyDown(Keys.R))
                SelectionType = ToggleSelectionType();
        }

        /// <summary>
        /// Handler for host's LayoutUpdated event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void host_LayoutUpdated(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, m_control.Width, m_control.Height);
        }

        private TriSelectionType ToggleSelectionType()
        {
            int intType = (int)SelectionType;

            if (SelectionType != TriSelectionType.Category)
                intType += 1;

            else
                intType = 0;

            return (TriSelectionType)intType;
        }
        #endregion
    }
}
