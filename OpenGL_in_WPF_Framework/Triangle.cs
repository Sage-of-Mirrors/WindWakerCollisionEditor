using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CollisionEditor
{
    public class Triangle : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Vector3 Vertex1
        {
            get { return m_vertex1; }
            set { m_vertex1 = value; }
        }

        private Vector3 m_vertex1;

        public Vector3 Vertex2
        {
            get { return m_vertex2; }
            set { m_vertex2 = value; }
        }

        private Vector3 m_vertex2;

        public Vector3 Vertex3
        {
            get { return m_vertex3; }
            set { m_vertex3 = value; }
        }

        private Vector3 m_vertex3;

        public Property FaceProperty
        {
            get { return m_faceProperty; }

            set
            {
                if (value != m_faceProperty)
                {
                    m_faceProperty = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Property m_faceProperty;

        public Group ParentGroup
        {
            get { return m_parentGroup; }

            set
            {
                if (value != m_parentGroup)
                {
                    m_parentGroup = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Group m_parentGroup;  

        public int GroupIndex;

        public bool IsSelected
        {
            get { return m_isSelected; }

            set
            {
                if (value != m_isSelected)
                {
                    m_isSelected = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private bool m_isSelected;

        public int[] vertIndices;

        public Triangle(Vector3 vert1, Vector3 vert2, Vector3 vert3, Property prop, int groupIndex)
        {
            m_vertex1 = vert1;

            m_vertex2 = vert2;

            m_vertex3 = vert3;

            m_faceProperty = prop;

            GroupIndex = groupIndex;
        }
    }
}
