using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using System.IO;
using Microsoft.Win32;
using GameFormatReader.Common;
using GameFormatReader.GCWii.Binaries.GC;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace CollisionEditor
{
    class ViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private Renderer m_renderer;

        private TreeView m_tree;

        public PropertyGrid test;

        public Group SelectedGroup
        {
            get { return m_selectedGroup; }

            set
            {
                if (value != m_selectedGroup)
                {
                    m_selectedGroup = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Group m_selectedGroup;

        public ObservableCollection<Category> Categories
        {
            get { return m_categories; }

            set
            {
                if (value != m_categories)
                {
                    m_categories = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public List<Triangle> m_selectedTris;

        public TriangleSelectionViewModel SelectedTriangles { get { return m_selectedViewTriangles; }
            set { m_selectedViewTriangles = value;  NotifyPropertyChanged(); }
        }

        private TriangleSelectionViewModel m_selectedViewTriangles;

        //Primary data structure
        private ObservableCollection<Category> m_categories;

        internal void CreateGraphicsContext(GLControl ctrl, WindowsFormsHost host, PropertyGrid grid)
        {
            m_renderer = new Renderer(ctrl, host);

            m_selectedTris = new List<Triangle>();

            SelectedTriangles = new TriangleSelectionViewModel(grid);

            m_renderer.SelectedTris += m_renderer_SelectedTris;

            m_renderer.RegroupTris += m_renderer_RegroupTris;

            m_renderer.RecategorizeTris += m_renderer_RecategorizeTris;

            m_renderer.FocusCamera += m_renderer_FocusCamera;
        }

        void m_renderer_FocusCamera(object sender, EventArgs e)
        {
            if (SelectedTriangles.SelectedItems.Count != 0)
                FocusCamera(new AABB(new List<Triangle>(SelectedTriangles.SelectedItems)));
        }

        void m_renderer_RecategorizeTris(object sender, EventArgs e)
        {
            if (Categories != null)
                RecategorizeSelectedTriangles();
        }

        void m_renderer_RegroupTris(object sender, EventArgs e)
        {
            if (Categories != null)
                RegroupSelectedTriangles();
        }

        void m_renderer_SelectedTris(object sender, SelectTriangleEventArgs e)
        {
            if (e.SelectedTris.Count != 0)
            {
                if (SelectedTriangles.SelectedItems.Count != 0)
                {
                    foreach (Triangle tri in SelectedTriangles.SelectedItems)
                    {
                        tri.IsSelected = false;
                    }

                    SelectedTriangles.SelectedItems.Clear();
                }

                foreach (Triangle tri in e.SelectedTris)
                {
                    tri.IsSelected = true;

                    SelectedTriangles.SelectedItems.Add(tri);
                }

                test.Update();
            }
        }

        internal void FocusCamera(AABB boundingBox)
        {
            m_renderer.Cam.SetCameraView(boundingBox, m_renderer.m_control.Width, m_renderer.m_control.Height);
        }

        public void Open()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "All Supported Types (*.dae, *.dzb, *.arc, *.rarc)|*.arc;*.rarc;*.dae;*.dzb|DAE Files (*.dae)|*.dae|DZB Files (*.dzb)|*.dzb|ARC Files (*.arc)|*.arc|RARC Files (*.rarc)|*.rarc";

            if (openFile.ShowDialog() == true)
            {
                string[] fileNameExtension = openFile.FileName.Split('.');

                if (fileNameExtension.Count() >= 2)
                {
                    switch (fileNameExtension[fileNameExtension.Count() - 1])
                    {
                        case "arc":
                        case "rarc":
                            break;
                        case "dae":
                            GetDaeData(openFile.FileName);
                            break;
                        case "dzb":
                            using (FileStream stream = new FileStream(openFile.FileName, FileMode.Open))
                            {
                                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                                GetDzbData(reader);
                            }
                            break;
                    }

                    FocusCameraInit();
                }
            }
        }

        internal void FocusCameraInit()
        {
            List<Triangle> allTris = new List<Triangle>();

            foreach(Category cat in Categories)
            {
                foreach (Group grp in cat.Groups)
                {
                    allTris.AddRange(grp.Triangles);
                }
            }

            AABB boundingBox = new AABB(allTris);

            m_renderer.Cam.SetCameraView(boundingBox, m_renderer.m_control.Width, m_renderer.m_control.Height);
        }

        public void Save()
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.Filter = "DZB files (*.dzb)|*.dzb";

            if (saveFile.ShowDialog() == true)
            {
                using (EndianBinaryWriter writer = new EndianBinaryWriter(new FileStream(saveFile.FileName, FileMode.Create), Endian.Big))
                {
                    Export(writer);
                }
            }
        }

        private void Export(EndianBinaryWriter writer)
        {

        }

        public void Close()
        {
            SelectedTriangles.SelectedItems.Clear();

            test.Update();

            Categories.Clear();

            m_renderer.RenderableObjs.Clear();
        }

        public void Delete()
        {

        }

        public void SetTreeView(TreeView tree)
        {
            m_tree = tree;

            m_tree.SelectedItemChanged += m_tree_SelectedItemChanged;
        }

        void m_tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((e.NewValue != null) && (e.NewValue.GetType() == typeof(Group)))
            {
                Group grp = e.NewValue as Group;

                SelectedGroup = grp;

                SelectTriangleEventArgs args = new SelectTriangleEventArgs();

                args.SelectedTris = new List<Triangle>(grp.Triangles);

                m_renderer_SelectedTris(this, args);
            }

            else
                SelectedGroup = null;
        }

        private void GetArcData()
        {

        }

        private void GetDaeData(string fileName)
        {
            DAE importedCol = new DAE(fileName);

            Categories = importedCol.GetCategories();

            AddRenderableObjs();
        }

        private void GetDzbData(EndianBinaryReader stream)
        {
            DZB nativeCol = new DZB(stream);

            Categories = nativeCol.GetCategories();

            AddRenderableObjs();
        }

        private void AddRenderableObjs()
        {
            foreach (Category cat in Categories)
            {
                foreach (Group grp in cat.Groups)
                    m_renderer.RenderableObjs.Add(grp);
            }
        }

        private void RegroupSelectedTriangles()
        {
            Group newGroup = new Group();

            bool isAllOneGroup = false;

            Group testGroup = SelectedTriangles.SelectedItems[0].ParentGroup;

            foreach (Triangle tri in SelectedTriangles.SelectedItems)
            {
                if (tri.ParentGroup != testGroup)
                {
                    isAllOneGroup = false;
                    break;
                }

                isAllOneGroup = true;
            }

            foreach (Triangle tri in SelectedTriangles.SelectedItems)
            {
                tri.ParentGroup.Triangles.Remove(tri);

                if (tri.ParentGroup.Triangles.Count == 0)
                {
                    RemoveDeadGroup(tri.ParentGroup);
                }

                tri.ParentGroup = newGroup;

                newGroup.Triangles.Add(tri);
            }

            newGroup.CreateBufferObjects();

            m_renderer.RenderableObjs.Add(newGroup);

            if (isAllOneGroup)
            {
                newGroup.GroupCategory = testGroup.GroupCategory;

                newGroup.GroupCategory.Groups.Add(newGroup);
            }

            else
            {
                newGroup.GroupCategory = Categories[0];

                Categories[0].Groups.Add(newGroup);
            }
        }

        private void RecategorizeSelectedTriangles()
        {
            Group newGroup = new Group();

            foreach (Triangle tri in SelectedTriangles.SelectedItems)
            {
                tri.ParentGroup.Triangles.Remove(tri);

                if (tri.ParentGroup.Triangles.Count == 0)
                {
                    RemoveDeadGroup(tri.ParentGroup);
                }

                tri.ParentGroup = newGroup;

                newGroup.Triangles.Add(tri);
            }

            newGroup.CreateBufferObjects();

            m_renderer.RenderableObjs.Add(newGroup);

            Category cat = new Category();

            newGroup.GroupCategory = cat;

            cat.Groups.Add(newGroup);

            Categories.Add(cat);
        }

        private void RemoveDeadGroup(Group group)
        {
            group.GroupCategory.Groups.Remove(group);

            if (group.GroupCategory.Groups.Count == 0)
            {
                RemoveDeadCategory(group.GroupCategory);
            }

            m_renderer.RenderableObjs.Remove(group);
        }

        private void RemoveDeadCategory(Category cat)
        {
            Categories.Remove(cat);
        }

        #region Nintendo-Specific

        internal RARC LoadRarcFile(string fileName)
        {
            string tempFileName = "";

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                string compressedFileTest = reader.ReadString(4);

                if (compressedFileTest == "Yay0")
                {
                    byte[] uncompressedArc = DecodeYay0(reader);

                    reader.Close();

                    tempFileName = System.IO.Path.GetTempFileName();

                    fileName = tempFileName;

                    FileInfo info = new FileInfo(fileName);

                    info.Attributes = FileAttributes.Temporary;

                    using (FileStream tempStream = new FileStream(tempFileName, FileMode.Create))
                    {
                        EndianBinaryWriter tempWriter = new EndianBinaryWriter(tempStream, Endian.Big);

                        tempWriter.Write(uncompressedArc);

                        tempWriter.Flush();

                        tempWriter.Close();
                    }
                }

                else if (compressedFileTest == "Yaz0")
                {
                    byte[] uncompressedArc = DecodeYaz0(reader);

                    reader.Close();

                    tempFileName = System.IO.Path.GetTempFileName();

                    fileName = tempFileName;

                    FileInfo info = new FileInfo(fileName);

                    info.Attributes = FileAttributes.Temporary;

                    using (FileStream tempStream = new FileStream(tempFileName, FileMode.Create))
                    {
                        EndianBinaryWriter tempWriter = new EndianBinaryWriter(tempStream, Endian.Big);

                        tempWriter.Write(uncompressedArc);

                        tempWriter.Flush();

                        tempWriter.Close();
                    }
                }
            }

            RARC loadedRarc = new RARC(fileName);

            if (File.Exists(tempFileName))
                File.Delete(tempFileName);

            return loadedRarc;
        }

        internal byte[] DecodeYay0(EndianBinaryReader reader)
        {
            int uncompressedSize = reader.ReadInt32();

            int linkTableOffset = reader.ReadInt32();

            int nonLinkedTableOffset = reader.ReadInt32();

            int maskBitCounter = 0;

            int currentOffsetInDestBuffer = 0;

            int currentMask = 0;

            byte[] uncompData = new byte[uncompressedSize];

            do
            {
                if (maskBitCounter == 0)
                {
                    currentMask = reader.ReadInt32();

                    maskBitCounter = 32;
                }

                if (((uint)currentMask & (uint)0x80000000) == 0x80000000)
                {
                    uncompData[currentOffsetInDestBuffer] = reader.ReadByteAt(nonLinkedTableOffset);

                    currentOffsetInDestBuffer++;

                    nonLinkedTableOffset++;
                }

                else
                {
                    ushort link = reader.ReadUInt16At(linkTableOffset);

                    linkTableOffset += 2;

                    int offset = currentOffsetInDestBuffer - (link & 0xfff);

                    int count = link >> 12;

                    if (count == 0)
                    {
                        byte countModifier;

                        countModifier = reader.ReadByteAt(nonLinkedTableOffset);

                        nonLinkedTableOffset++;

                        count = countModifier + 18;
                    }

                    else
                        count += 2;

                    int blockCopy = offset;

                    for (int i = 0; i < count; i++)
                    {
                        uncompData[currentOffsetInDestBuffer] = uncompData[blockCopy - 1];

                        currentOffsetInDestBuffer++;

                        blockCopy++;
                    }
                }

                currentMask <<= 1;

                maskBitCounter--;

            } while (currentOffsetInDestBuffer < uncompressedSize);

            return uncompData;
        }

        internal byte[] DecodeYaz0(EndianBinaryReader reader)
        {
            int uncompressedSize = reader.ReadInt32();

            byte[] dest = new byte[uncompressedSize];

            int srcPlace = 0x10, dstPlace = 0; //current read/write positions

            int validBitCount = 0; //number of valid bits left in "code" byte

            byte currCodeByte = 0;

            while (dstPlace < uncompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    currCodeByte = reader.ReadByteAt(srcPlace);

                    ++srcPlace;

                    validBitCount = 8;
                }

                if ((currCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dest[dstPlace] = reader.ReadByteAt(srcPlace);

                    dstPlace++;

                    srcPlace++;
                }

                else
                {
                    //RLE part
                    byte byte1 = reader.ReadByteAt(srcPlace);

                    byte byte2 = reader.ReadByteAt(srcPlace + 1);

                    srcPlace += 2;

                    int dist = ((byte1 & 0xF) << 8) | byte2;

                    int copySource = dstPlace - (dist + 1);

                    int numBytes = byte1 >> 4;

                    if (numBytes == 0)
                    {
                        numBytes = reader.ReadByteAt(srcPlace) + 0x12;
                        srcPlace++;
                    }

                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dest[dstPlace] = dest[copySource];

                        copySource++;

                        dstPlace++;
                    }
                }

                //use next bit from "code" byte
                currCodeByte <<= 1;

                validBitCount -= 1;
            }

            return dest;
        }

        #endregion

        #region Command Callbacks

        /// <summary> The user has requested to open a new map, ask which map and then unload current if needed. </summary>
        public ICommand OnRequestMapOpen
        {
            get { return new RelayCommand(x => Open()); }
        }

        /// <summary> The user has requested to save the currently open map. Only available if a map is currently opened. </summary>
        public ICommand OnRequestMapSave
        {
            get { return new RelayCommand(x => Save(), x => m_categories != null); }
        }

        /// <summary> The user has requested to unload the currently open map. Only available if a map is currently opened. Ask user if they'd like to save. </summary>
        public ICommand OnRequestMapClose
        {
            get { return new RelayCommand(x => Close(), x => Categories != null); }
        }

        /// <summary> The user has requested to undo the last action. Only available if they've made an undoable action. </summary>
        public ICommand OnRequestUndo
        {
            get { return new RelayCommand(x => { return; }, x => false); }
        }

        /// <summary> The user has requested to redo the last undo action. Only available if they've undone an action. </summary>
        public ICommand OnRequestRedo
        {
            get { return new RelayCommand(x => { return; }, x => false); }
        }

        /// <summary> Delete the currently selected objects in the world. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestRegroupTris
        {
            get { return new RelayCommand(x => RegroupSelectedTriangles(), x => Categories != null); }
        }

        /// <summary> Delete the currently selected objects in the world. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestRecategorizeTris
        {
            get { return new RelayCommand(x => RecategorizeSelectedTriangles(), x => Categories != null); }
        }

        /// <summary> Delete the currently selected objects in the world. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestDelete
        {
            get { return new RelayCommand(x => Delete(), x => false); }
        }

        /// <summary> The user has pressed Alt + F4, chosen Exit from the File menu, or clicked the close button. </summary>
        public ICommand OnRequestApplicationClose
        {
            get { return new RelayCommand(x => Application.Current.MainWindow.Close()); }
        }

        public IEnumerable<TerrainType> TerrainTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(TerrainType))
                    .Cast<TerrainType>();
            }
        }

        #endregion
    }

    public class NullToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
