using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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

        #region Properties

        #region Bitfield 1

        #region CameraID

        private int m_camID;

        /// <summary>
        /// Camera ID. Purpose unknown.
        /// </summary>
        public int CameraID
        {
            get { return m_camID; }
            set
            {
                if (value != m_camID)
                {
                    m_camID = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region ExitIndex

        private int m_exitID;

        /// <summary>
        /// The index of an SCLS entry to use to exit the map. If the index is 63/0x3F, no exit is triggered.
        /// </summary>
        public virtual int ExitIndex
        {
            get { return m_exitID; }
            set
            {
                if (value != m_exitID)
                {
                    m_exitID = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region SoundID

        private int m_soundID;

        /// <summary>
        /// The sound that plays when an actor walks across the face.
        /// </summary>
        public int SoundID
        {
            get { return m_soundID; }
            set
            {
                if (value != m_soundID)
                {
                    m_soundID = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region PolyColor

        private int m_polyColor;

        /// <summary>
        /// The index of an entry related to ENVR & Co. Colo?
        /// </summary>
        public int PolyColor
        {
            get { return m_polyColor; }
            set
            {
                if (value != m_polyColor)
                {
                    m_polyColor = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #endregion

        #region Bitfield 2

        #region Attribute Code

        private int m_attribCode;

        public int AttributeCode
        {
            get { return m_attribCode; }
            set
            {
                if (value != m_attribCode)
                {
                    m_attribCode = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region Ground Code

        private int m_groundCode;

        public int GroundCode
        {
            get { return m_groundCode; }
            set
            {
                if (value != m_groundCode)
                {
                    m_groundCode = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #region Link Number

        private int m_linkNo;

        public int LinkNumber
        {
            get { return m_linkNo; }
            set
            {
                if (value != m_linkNo)
                {
                    m_linkNo = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #region Special Code

        private int m_specialCode;

        public int SpecialCode
        {
            get { return m_specialCode; }
            set
            {
                if (value != m_specialCode)
                {
                    m_specialCode = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #region Wall Code

        private int m_wallCode;

        public int WallCode
        {
            get { return m_wallCode; }
            set
            {
                if (value != m_wallCode)
                {
                    m_wallCode = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #endregion

        #region Bitfield 3

        #region Camera Move BG

        private int m_camMoveBg;

        public int CamMoveBG
        {
            get { return m_camMoveBg; }
            set
            {
                if (value != m_camMoveBg)
                {
                    m_camMoveBg = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #region Room Camera ID

        private int m_roomCamID;

        public int RoomCamID
        {
            get { return m_roomCamID; }
            set
            {
                if (value != m_roomCamID)
                {
                    m_roomCamID = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #region Room Path ID

        private int m_roomPathID;

        public int RoomPathID
        {
            get { return m_roomPathID; }

            set
            {
                m_roomPathID = value;

                NotifyPropertyChanged();
            }
        }


        #endregion

        #region Room Path Point Number

        private int m_roomPathPntNo;

        public int RoomPathPointNo
        {
            get { return m_roomPathPntNo; }
            set
            {
                if (value != m_roomPathPntNo)
                {
                    m_roomPathPntNo = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #endregion

        #region Camera Behavior

        private int m_cameraBehavior;

        public int CameraBehavior
        {
            get { return m_cameraBehavior; }
            set
            {
                if (value != m_cameraBehavior)
                {
                    m_cameraBehavior = value;

                    NotifyPropertyChanged();
                }
            }
        }


        #endregion

        #endregion

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

        public Triangle()
        {

        }

        public Triangle(Vector3 vert1, Vector3 vert2, Vector3 vert3, Property prop, int groupIndex)
        {
            m_vertex1 = vert1;

            m_vertex2 = vert2;

            m_vertex3 = vert3;

            GroupIndex = groupIndex;

            m_camID = prop.CameraID;
            m_soundID = prop.SoundID;
            m_exitID = prop.ExitIndex;
            m_polyColor = prop.PolyColor;

            m_linkNo = prop.LinkNumber;
            m_wallCode = prop.WallCode;
            m_specialCode = prop.SpecialCode;
            m_attribCode = prop.AttributeCode;
            m_groundCode = prop.GroundCode;

            m_camMoveBg = prop.CamMoveBG;
            m_roomCamID = prop.RoomCamID;
            m_roomPathID = prop.RoomPathID;
            m_roomPathPntNo = prop.RoomPathPointNo;

            m_cameraBehavior = prop.CameraBehavior;
        }
    }

    public class TriangleSelectionViewModel : Triangle
    {
        public ObservableCollection<Triangle> SelectedItems { get; set; }

        public TriangleSelectionViewModel()
        {
            SelectedItems = new ObservableCollection<Triangle>();

            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
                if (e.NewItems != null)
                {
                    foreach (Triangle tri in e.NewItems)
                    {
                        tri.PropertyChanged += Item_PropertyChanged;
                    }
                }

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    foreach (Triangle tri in SelectedItems)
                    {
                        tri.PropertyChanged -= Item_PropertyChanged;
                    }
                }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }

        public override int ExitIndex
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.ExitIndex))
                    return SelectedItems[0].ExitIndex;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.ExitIndex = value;
                }

                NotifyPropertyChanged();
            }
        }
    }

    public static class IEnumerableExtensions
    {
        // Extension method for IEnumerable
        public static bool IsSameValue<T, U>(this IEnumerable<T> list, Func<T, U> selector)
        {
            return list.Select(selector).Distinct().Count() == 1;
        }
    }
}
