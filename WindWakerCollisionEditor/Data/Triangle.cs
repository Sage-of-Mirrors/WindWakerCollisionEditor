using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid;
using OpenTK;

namespace WindWakerCollisionEditor
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
        public virtual int CameraID
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

        private SoundID m_soundID;

        /// <summary>
        /// The sound that plays when an actor walks across the face.
        /// </summary>
        public virtual SoundID SoundID
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
        public virtual int PolyColor
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
        /*
        private int m_attribCode;

        public virtual int AttributeCode
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
        }*/

        private AttributeCode m_attribCode;

        public virtual AttributeCode AttributeCode
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

        /*
        private int m_groundCode;

        public virtual int GroundCode
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
        }*/

        private GroundCode m_groundCode;

        public virtual GroundCode GroundCode
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

        public virtual int LinkNumber
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

        private SpecialCode m_specialCode;

        public virtual SpecialCode SpecialCode
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

        /*
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
        }*/

        private WallCode m_wallCode;

        public virtual WallCode WallCode
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

        public virtual int CamMoveBG
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

        public virtual int RoomCamID
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

        public virtual int RoomPathID
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

        public virtual int RoomPathPointNo
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

        public virtual int CameraBehavior
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

        public Vector3 GetCenter()
        {
            return new Vector3(
                (Vertex1.X + Vertex2.X + Vertex3.X) / 3,
                (Vertex1.Y + Vertex2.Y + Vertex3.Y) / 3,
                (Vertex1.Z + Vertex2.Z + Vertex3.Z) / 3);
        }
    }

    public class TriangleSelectionViewModel : Triangle
    {
        public ObservableCollection<Triangle> SelectedItems { get; set; }

        private PropertyGrid PropGrid;

        private bool m_hasSelection;

        public bool HasSelection
        {
            get { return m_hasSelection; }

            set
            {
                if (value != m_hasSelection)
                {
                    m_hasSelection = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public TriangleSelectionViewModel()
        {
            SelectedItems = new ObservableCollection<Triangle>();

            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        public TriangleSelectionViewModel(PropertyGrid propGrid)
        {
            SelectedItems = new ObservableCollection<Triangle>();

            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            PropGrid = propGrid;
        }

        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
                if (e.NewItems != null)
                {
                    HasSelection = true;

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

        public override AttributeCode AttributeCode
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return AttributeCode.None;
                if (SelectedItems.IsSameValue(i => i.AttributeCode))
                {
                    return SelectedItems[0].AttributeCode;
                }

                else
                {
                    return AttributeCode.Multi;
                }
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.AttributeCode = value;
                }

                NotifyPropertyChanged();
            }
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

        public override GroundCode GroundCode
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return GroundCode.None;
                if (SelectedItems.IsSameValue(i => i.GroundCode))
                    return SelectedItems[0].GroundCode;
                return GroundCode.Multi;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.GroundCode = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override WallCode WallCode
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return WallCode.None;
                if (SelectedItems.IsSameValue(i => i.WallCode))
                    return SelectedItems[0].WallCode;
                return WallCode.Multi;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.WallCode = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int LinkNumber
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.LinkNumber))
                    return SelectedItems[0].LinkNumber;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.LinkNumber = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override SpecialCode SpecialCode
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return SpecialCode.None;
                if (SelectedItems.IsSameValue(i => i.SpecialCode))
                    return SelectedItems[0].SpecialCode;
                return SpecialCode.Multi;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.SpecialCode = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int RoomCamID
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.CameraID))
                    return SelectedItems[0].CameraID;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.CameraID = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int CamMoveBG
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.CamMoveBG))
                    return SelectedItems[0].CamMoveBG;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.CamMoveBG = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int RoomPathID
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.RoomPathID))
                    return SelectedItems[0].RoomPathID;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.RoomPathID = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int RoomPathPointNo
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.RoomPathPointNo))
                    return SelectedItems[0].RoomPathPointNo;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.RoomPathPointNo = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int PolyColor
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.PolyColor))
                    return SelectedItems[0].PolyColor;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.PolyColor = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override SoundID SoundID
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return SoundID.None;
                if (SelectedItems.IsSameValue(i => i.SoundID))
                    return SelectedItems[0].SoundID;
                return SoundID.Multi;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.SoundID = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int CameraID
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.CameraID))
                    return SelectedItems[0].CameraID;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.CameraID = value;
                }

                NotifyPropertyChanged();
            }
        }

        public override int CameraBehavior
        {
            get
            {
                if (SelectedItems.Count == 0)
                    return -1;
                if (SelectedItems.IsSameValue(i => i.CameraBehavior))
                    return SelectedItems[0].CameraBehavior;
                return -1;
            }

            set
            {
                foreach (Triangle tri in SelectedItems)
                {
                    tri.CameraBehavior = value;
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
