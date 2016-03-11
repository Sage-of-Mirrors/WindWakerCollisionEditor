using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;

namespace WindWakerCollisionEditor
{
    public class Camera : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private float m_moveSpeed = 5000.0f;

        public float MoveSpeed { get { return m_moveSpeed; } 
            set
            {
                if (value != m_moveSpeed)
                {
                    m_moveSpeed = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public Matrix4 ViewMatrix
        {
            get { return Matrix4.LookAt(eye, target, Vector3.UnitY); }
        }

        public Vector3 EyePos
        {
            get { return eye; }
            set { eye = value; }
        }

        Vector3 eye = new Vector3(0, 0, 0);

        public Vector3 TargetPos
        {
            get { return target; }
            set { target = value; }
        }

        Vector3 target = Vector3.Zero;

        Transform Trans = new Transform();

        private Stopwatch m_time;

        private float m_deltaTime;

        public Camera()
        {
            m_time = new Stopwatch();
        }

        public void Update()
        {
            m_deltaTime = m_time.ElapsedMilliseconds / 1000f;

            m_time.Restart();

            Vector3 moveDir = Vector3.Zero;

            if (Input.GetKey(Keys.W))
            {
                moveDir += Vector3.UnitZ;
            }

            if (Input.GetKey(Keys.S))
            {
                moveDir -= Vector3.UnitZ;
            }

            if (Input.GetKey(Keys.A))
            {
                moveDir += Vector3.UnitX;
            }

            if (Input.GetKey(Keys.D))
            {
                moveDir -= Vector3.UnitX;
            }

            if (Input.GetMouseButton(1))
            {
                Rotate(Input.MouseDelta.X, Input.MouseDelta.Y);
            }

            float moveSpeed = Input.GetKey(Keys.Space) ? MoveSpeed * 10f : MoveSpeed;

            int scrollSensitivity = 10;

            // Normalize the move direction
            moveDir.NormalizeFast();

            // Make it relative to the current rotation.
            moveDir = Trans.Rotation.Multiply(moveDir);

            Trans.Position += Vector3.Multiply(moveDir, moveSpeed) * m_deltaTime;

            Trans.Position += Trans.Forward * Input.MouseScrollDelta * m_deltaTime * scrollSensitivity;

            eye = Trans.Position;

            target = Trans.Position + Trans.Forward;
        }

        public void SetCameraView(AABB boundingBox, float width, float height)
        {
            float halfFOVRadians = MathHelper.DegreesToRadians(65/2);

            float radius = (0.5f * (boundingBox.Max - boundingBox.Min)).Length;

            //if (width / height > 1.0f)
                //radius *= (width / height);

            float distanceFromBoundingSphere = (float)(radius / Math.Tan(halfFOVRadians));

            Vector3 camOffset = Vector3.Multiply(Vector3.Normalize(Trans.Forward), -distanceFromBoundingSphere);

            //Trans.LookAt(boundingBox.Center);

            Trans.Position = (camOffset + boundingBox.Center);
        }

        private void Rotate(float x, float y)
        {
            Trans.Rotate(Vector3.UnitY, -x);
            Trans.Rotate(Trans.Right, y);

            // Clamp them from looking over the top point.
            Vector3 up = Vector3.Cross(Trans.Forward, Trans.Right);
            if (Vector3.Dot(up, Vector3.UnitY) < 0.01f)
            {
                Trans.Rotate(Trans.Right, -y);
            }
        }

        #region Raycasting

        internal Vector3 CastRay(int mouseX, int mouseY, float controlWidth, float controlHeight, Matrix4 projMatrix)
        {
            Vector3 normDevCoordsRay = new Vector3((2.0f * mouseX) / controlWidth - 1.0f,
                1.0f - (2.0f * mouseY) / controlHeight, -1.0f);

            Vector4 clipRay = new Vector4(normDevCoordsRay, 1.0f);

            Vector4 eyeRay = Vector4.Transform(clipRay, Matrix4.Invert(projMatrix));

            eyeRay = new Vector4(eyeRay.X, eyeRay.Y, -1, 0);

            Vector3 unNormalizedRay = new Vector3(Vector4.Transform(eyeRay, Matrix4.Invert(ViewMatrix)).Xyz);

            return Vector3.Normalize(unNormalizedRay);
        }

        internal Color4 CheckHitAxisAlignedBoundingBox(Vector3 eye, Vector3 ray, Vector3 lowerBound, Vector3 upperBound)
        {
            Vector3 dirFrac = new Vector3(1.0f / ray.X, 1.0f / ray.Y, 1.0f / ray.Z);

            float t1 = (lowerBound.X - eye.X) * dirFrac.X;
            float t2 = (upperBound.X - eye.X) * dirFrac.X;
            float t3 = (lowerBound.Y - eye.Y) * dirFrac.Y;
            float t4 = (upperBound.Y - eye.Y) * dirFrac.Y;
            float t5 = (lowerBound.Z - eye.Z) * dirFrac.Z;
            float t6 = (upperBound.Z - eye.Z) * dirFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0)
                return Color4.Yellow;

            if (tmin > tmax)
                return Color4.Yellow;

            else
                return Color4.Red;
        }

        internal void CheckHitBoundingSphere(Vector3 eye, Vector3 ray, float radius)
        {
            Vector3 position = new Vector3();

            float b = Vector3.Dot(ray, (eye - position));

            float c = Vector3.Dot((eye - position), (eye - position));

            c = c - radius;

            float a = (b * b) - c;

            //if (a >= 0)
            //debugRayColor = Color4.Red;

            //else
            //debugRayColor = Color4.Yellow;
        }

        internal Triangle CastAgainstTriangle(int mouseX, int mouseY, 
            int controlWidth, int controlHeight, Matrix4 projMatrix, List<Group> grps)
        {
            //Vector3 ray = CastRay(mouseX, mouseY, controlWidth, controlHeight, projMatrix);

            Vector3 ray = ViewportPointToRay(new Vector3(mouseX, mouseY, 0), projMatrix, controlWidth, controlHeight);

            float smallestTime = float.MaxValue;

            Triangle closestTri = null;

            foreach (Group group in grps)
            {
                foreach (Triangle tri in group.Triangles)
                {
                    float time;

                    if (CheckHitTriangle(ray, tri, out time))
                    {
                        if ((time >= 0) && (time < smallestTime))
                        {
                            smallestTime = time;

                            closestTri = tri;
                        }
                    }
                }
            }
            return closestTri;
        }

        internal bool CheckHitTriangle(Vector3 ray, Triangle tri, out float time)
        {
            //Vector3 normal = Vector3.Cross(Vector3.Normalize((tri.Vertex1 - tri.Vertex2)), Vector3.Normalize((tri.Vertex2 - tri.Vertex3)));

            //normal = Vector3.Normalize(normal);

            Vector3 normal = new Vector3();

            Vector3 V = tri.Vertex2 - tri.Vertex1;

            Vector3 W = tri.Vertex3 - tri.Vertex1;

            normal.X = (V.Y * W.Z) - (V.Z * W.Y);

            normal.Y = (V.Z * W.X) - (V.X * W.Z);

            normal.Z = (V.X * W.Y) - (V.Y * W.X);

            normal = Vector3.Normalize(normal);

            Vector3 triCenter = new Vector3();

            //Calculate the center of the triangle so we can use it as the point to test against the ray
            triCenter.X = (tri.Vertex1.X + tri.Vertex2.X + tri.Vertex3.X) / 3;

            triCenter.Y = (tri.Vertex1.Y + tri.Vertex2.Y + tri.Vertex3.Y) / 3;

            triCenter.Z = (tri.Vertex1.Z + tri.Vertex2.Z + tri.Vertex3.Z) / 3;

            //Normalizing the subtraction between eye and tricenter is BAD.
            float numerator = (Vector3.Dot(normal, Vector3.Subtract(eye, triCenter)));

            float denominator = Vector3.Dot(ray, normal);

            //This is the time it takes for the ray to intersect with the plane that the triangle lies on.
            float t = (-(numerator)/denominator);

            Vector3 point = eye + Vector3.Normalize((t * ray));

            //t = time when you do the calculation right!
            time = t;

            //if (Vector3.Dot(normal, eye) < 0.0f)
                //return false;

            if (t < 0)
                return false;

            Vector3 vec1 = Vector3.Normalize(tri.Vertex1 - point);

            Vector3 vec2 = Vector3.Normalize(tri.Vertex2 - point);

            Vector3 vec3 = Vector3.Normalize(tri.Vertex3 - point);

            Vector3 n4 = Vector3.Cross(vec3, vec2);

            Vector3 n5 = Vector3.Cross(vec2, vec1);

            Vector3 n6 = Vector3.Cross(vec1, vec3);

            n4 = Vector3.Normalize(n4);

            n5 = Vector3.Normalize(n5);

            n6 = Vector3.Normalize(n6);

            float dist1 = Vector3.Dot(-eye,n4);

            float dist2 = Vector3.Dot(-eye,n5);

            float dist3 = Vector3.Dot(-eye,n6);

            if ((Vector3.Dot(point, n4) + dist1) < 0)
                return false;

            if ((Vector3.Dot(point, n5) + dist2) < 0)
                return false;

            if ((Vector3.Dot(point, n6) + dist3) < 0)
                return false;

            else
                return true;
        }

        public Vector3 ViewportPointToRay(Vector3 mousePos, Matrix4 projectionMatrix, int width, int height)
        {
            Vector3 mousePosA = new Vector3(mousePos.X, mousePos.Y, 0f);
            Vector3 mousePosB = new Vector3(mousePos.X, mousePos.Y, 1f);


            Vector4 nearUnproj = UnProject(projectionMatrix, ViewMatrix, mousePosA, width, height);
            Vector4 farUnproj = UnProject(projectionMatrix, ViewMatrix, mousePosB, width, height);

            Vector3 dir = farUnproj.Xyz - nearUnproj.Xyz;
            dir.Normalize();

            return dir;
        }


        public Vector4 UnProject(Matrix4 projection, Matrix4 view, Vector3 mouse, int width, int height)
        {
            Vector4 vec = new Vector4();

            vec.X = 2.0f * mouse.X / width - 1;
            vec.Y = -(2.0f * mouse.Y / height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec;
        }

        #endregion
    }

    public class Transform
    {
        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Vector3 Right
        {
            get { return Rotation.Multiply(Vector3.UnitX); }
        }

        public Vector3 Forward
        {
            get { return Rotation.Multiply(Vector3.UnitZ); }
        }

        public Vector3 Up
        {
            get { return Rotation.Multiply(Vector3.UnitY); }
        }

        public void LookAt(Vector3 worldPosition)
        {
            Rotation = Quaternion.FromAxisAngle(Vector3.Normalize((Position - worldPosition)), 0f);
        }

        public void Rotate(Vector3 axis, float angleInDegrees)
        {
            Quaternion rotQuat = Quaternion.FromAxisAngle(axis, MathHelper.DegreesToRadians(angleInDegrees));
            Rotation = rotQuat * Rotation;
        }

        public void Translate(Vector3 amount)
        {
            Position += amount;
        }
    }
}
