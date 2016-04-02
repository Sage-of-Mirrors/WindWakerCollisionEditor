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
//using GameFormatReader.GCWii.Binaries.GC;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Common;

namespace WindWakerCollisionEditor
{
    #region Data Converters
    /// <summary>
    /// Null-to-Bool converter. If an object is null, Convert returns false.
    /// </summary>
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

    /// <summary>
    /// Zero-to-Bool converter. If the input value is 0, Convert returns false.
    /// </summary>
    public class ZeroToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(ObservableCollection<Triangle>))
            {
                ObservableCollection<Triangle> forCount = value as ObservableCollection<Triangle>;

                return forCount.Count != 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Null-to-Visibilty converter. If the input value is null, Convert returns Visibility.Hidden.
    /// </summary>
    public class NullToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Hidden;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

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

        #region Properties
        #region Bools

        /// <summary>
        /// Tells if a file is currently loaded.
        /// </summary>
        private bool isDataLoaded;

        #endregion

        #region Controls

        /// <summary>
        /// Represents the TreeView control on the main window.
        /// </summary>
        private TreeView m_tree;

        /// <summary>
        /// Represents the PropertyGrid on the main window.
        /// </summary>
        public PropertyGrid PropGrid;

        /// <summary>
        /// Represents the RecentFileList in the File menu on the main window.
        /// </summary>
        private RecentFileList m_recentFileList;

        #endregion

        #region Data
        /// <summary>
        /// Contains all of the data loaded from a file.
        /// </summary> 
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

        private ObservableCollection<Category> m_categories; 

        /// <summary>
        /// The currently selected Group.
        /// </summary>
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

        /// <summary>
        /// The currently selected Category.
        /// </summary>
        public Category SelectedCategory
        {
            get { return m_selectedCategory; }

            set
            {
                if (value != m_selectedCategory)
                {
                    m_selectedCategory = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Category m_selectedCategory;

        /// <summary>
        /// Represents the currently selected Triangles.
        /// It is in this format in order to support multiselection of Triangles
        /// in the PropertyGrid.
        /// </summary>
        public TriangleSelectionViewModel SelectedTriangles
        {
            get { return m_selectedViewTriangles; }
            set
            {
                if (value != m_selectedViewTriangles)
                {
                    m_selectedViewTriangles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private TriangleSelectionViewModel m_selectedViewTriangles;

        private UndoRedoManager m_undoRedoManager;

        #endregion

        #region Rendering

        /// <summary>
        /// Represents the OpenGL viewport and its associated Camera.
        /// </summary>
        public Renderer Renderer
        {
            get { return m_renderer; }
            set
            {
                if (value != m_renderer)
                {
                    m_renderer = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Renderer m_renderer;

        #endregion

        #region Strings

        /// <summary>
        /// The filename of the currently loaded file.
        /// </summary>
        public string CurrentFile
        {
            get { return m_currentFile; }

            set
            {
                if (value != m_currentFile)
                {
                    m_currentFile = value;

                    WindowTitle = m_currentFile;

                    NotifyPropertyChanged();
                }
            }
        }

        private string m_currentFile;

        /// <summary>
        /// The string displayed as the main window's title.
        /// </summary>
        public string WindowTitle
        {
            get { return m_windowTitle; }

            set
            {
                if (value != m_windowTitle)
                {
                    m_windowTitle = value + " - Wind Waker Collision Editor";

                    NotifyPropertyChanged();
                }
            }
        }

        private string m_windowTitle;

        #endregion
        #endregion 

        #region Initialization
        /// <summary>
        /// Initializes the OpenGL viewport and other important components.
        /// </summary>
        /// <param name="ctrl">The GLControl to initialize the viewport with</param>
        /// <param name="host">The host of the GLControl</param>
        /// <param name="grid">The PropertyGrid to associate with this ViewModel</param>
        /// <param name="fileList">The RecentFileList to associate with this ViewModel</param>
        public void CreateGraphicsContext(GLControl ctrl, WindowsFormsHost host, RecentFileList fileList)
        {
            Renderer = new Renderer(ctrl, host);

            SelectedTriangles = new TriangleSelectionViewModel();

            m_renderer.SelectedTris += m_renderer_SelectedTris;

            m_renderer.RegroupTris += m_renderer_RegroupTris;

            m_renderer.RecategorizeTris += m_renderer_RecategorizeTris;

            m_renderer.FocusCamera += m_renderer_FocusCamera;

            m_undoRedoManager = new UndoRedoManager();

            m_recentFileList = fileList;

            m_recentFileList.MenuClick += (s, e) => Open(e.Filepath);

            CurrentFile = "";
        }
        #endregion

        #region File Input
        /// <summary>
        /// Opens an OpenFileDialog and passes the filename specified by the user to Open(string fileName).
        /// </summary>
        public void OpenFileFromDialog()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "All Supported Types (*.dae, *.obj, *.dzb, *.arc, *.rarc)|*.arc;*.rarc;*.dae;*.dzb;*.obj|DAE Files (*.dae)|*.dae|OBJ Files (*.obj)|*.obj|DZB Files (*.dzb)|*.dzb|ARC Files (*.arc)|*.arc|RARC Files (*.rarc)|*.rarc";

            if (openFile.ShowDialog() == true)
            {
                Open(openFile.FileName);
            }
        }

        /// <summary>
        /// Reads the file specified by fileName into the program.
        /// </summary>
        /// <param name="fileName">File to read into the program</param>
        public void Open(string fileName)
        {
            if (isDataLoaded)
            {
                Close();
            }

            IModelSource source = null;

            try
            {
                string[] fileNameExtension = fileName.Split('.');

                if (fileNameExtension.Count() >= 2)
                {
                    switch (fileNameExtension[fileNameExtension.Count() - 1])
                    {
                        case "arc":
                        case "rarc":
                            source = new ARC();
                            break;
                        case "dae":
                            source = new DAE();
                            break;
                        case "dzb":
                            source = new DZB();
                            break;
                        case "obj":
                            source = new OBJ();
                            break;
                        default:
                            Console.WriteLine("Unknown file type " + 
                                fileNameExtension[fileNameExtension.Count() - 1] + ". Aborting...");
                            return;
                    }

                    Categories = (ObservableCollection<Category>)source.Load(fileName);

                    // Probably a hotfix. If the user tried to open an archive, but the archive didn't have any DZBs in it,
                    // the archive loader will return null.
                    if (Categories == null)
                    {
                        MessageBox.Show("The archive you opened did not contain any DZB collision files.", "No DZB files found");

                        return;
                    }

                    AddRenderableObjs();

                    FocusCameraAll();

                    CurrentFile = fileName;

                    m_recentFileList.InsertFile(CurrentFile);

                    isDataLoaded = true;
                }
            }

            catch
            {
                if (m_recentFileList.RecentFiles.Contains(fileName))
                {
                    MessageBox.Show("The file '" + fileName + "' no longer exists. It will be removed from the recently used files.", "File Not Found");

                    m_recentFileList.RemoveFile(fileName);
                }
            }
        }

        void cat_UndoRedoCommandEventArgs(object sender, UndoRedoEventArgs e)
        {
            m_undoRedoManager.AddUndoableCommand(e.cmd);
        }
        #endregion

        #region File Output
        /// <summary>
        /// Saves the currently open file to the filename that it was opened from.
        /// If the file was not originally .dzb, SaveAs() is invoked instead.
        /// </summary>
        /// <returns>Returns a bool specifiying if the save operation was successful</returns>
        public bool Save()
        {
            if (!CurrentFile.EndsWith("dzb"))
            {
                SaveAs();

                return true;
            }

            Export(CurrentFile);

            return true;
        }

        /// <summary>
        /// Opens a SaveFileDialog and passes the filename specified by the user to Export(string fileName).
        /// </summary>
        /// <returns>Returns a bool specifiying if the save operation was successful</returns>
        public bool SaveAs()
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.Filter = "DZB files (*.dzb)|*.dzb";

            if (saveFile.ShowDialog() == true)
            {
                Export(saveFile.FileName.Split('.')[0] + ".dzb");

                m_recentFileList.InsertFile(saveFile.FileName.Split('.')[0] + ".dzb");

                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// Prepares the current Category list by passing it to a new instance of DZBExporter and writes it to disk.
        /// </summary>
        /// <param name="fileName">File to save data to</param>
        private void Export(string fileName)
        {
            DZBExporter expo = new DZBExporter(new List<Category>(Categories), fileName);

            using (EndianBinaryWriter writer = new EndianBinaryWriter(new FileStream(fileName, FileMode.Create), Endian.Big))
            {
                try
                {
                    expo.Export(writer);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Asks the user if they would like to save the currently open file.
        /// </summary>
        /// <returns>Returns a bool specifying if the user chose to save the file.</returns>
        private bool AskSaveFile()
        {
            if (Categories != null)
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save the currently open mesh?", "Save current file?", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    return Save();
                }

                else if (result == MessageBoxResult.No)
                {
                    return true;
                }

                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return false;
        }
        #endregion

        #region Camera Focusing
        /// <summary>
        /// Handler for the FocusCamera event from Renderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_renderer_FocusCamera(object sender, EventArgs e)
        {
            if (SelectedTriangles.SelectedItems.Count != 0)
                FocusCameraSelected(new AABB(new List<Triangle>(SelectedTriangles.SelectedItems)));
        }

        /// <summary>
        /// Focuses the camera on the current selection.
        /// </summary>
        /// <param name="boundingBox">Bounding box of the currently selected triangles</param>
        internal void FocusCameraSelected(AABB boundingBox)
        {
            m_renderer.Cam.SetCameraView(boundingBox, m_renderer.m_control.Width, m_renderer.m_control.Height);
        }

        /// <summary>
        /// Focuses the camera on the entire mesh.
        /// </summary>
        internal void FocusCameraAll()
        {
            List<Triangle> allTris = new List<Triangle>();

            foreach (Category cat in Categories)
            {
                foreach (Group grp in cat.Groups)
                {
                    allTris.AddRange(grp.Triangles);
                }
            }

            AABB boundingBox = new AABB(allTris);

            m_renderer.Cam.SetCameraView(boundingBox, m_renderer.m_control.Width, m_renderer.m_control.Height);
        }
        #endregion

        #region TreeView
        /// <summary>
        /// Sets m_tree to the specified TreeView instance.
        /// </summary>
        /// <param name="tree">TreeView instance to associate with this ViewModel</param>
        public void SetTreeView(TreeView tree)
        {
            m_tree = tree;

            m_tree.SelectedItemChanged += m_tree_SelectedItemChanged;
        }

        /// <summary>
        /// Handler for selection of items in m_tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedCategory = null;
            SelectedGroup = null;

            if ((e.NewValue != null) && (e.NewValue.GetType() == typeof(Group)))
            {
                Group grp = e.NewValue as Group;

                SelectedGroup = grp;

                SelectTriangleEventArgs args = new SelectTriangleEventArgs();

                args.SelectedTris = new List<Triangle>(grp.Triangles);

                m_renderer_SelectedTris(this, args);
            }

            if ((e.NewValue != null) && (e.NewValue.GetType() == typeof(Category)))
            {
                SelectedCategory = (Category)e.NewValue;

                SelectTriangleEventArgs args = new SelectTriangleEventArgs();

                args.SelectedTris = new List<Triangle>();

                foreach (Group grp in SelectedCategory.Groups)
                {
                    foreach (Triangle tri in grp.Triangles)
                        args.SelectedTris.Add(tri);
                }

                m_renderer_SelectedTris(this, args);
            }

            else
                SelectedCategory = null;
        }
        #endregion

        #region Triangle Regrouping
        /// <summary>
        /// Handler for the RegroupTris event from Renderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_renderer_RegroupTris(object sender, EventArgs e)
        {
            if (Categories != null)
                RegroupSelectedTriangles();
        }

        /// <summary>
        /// Handler for the RecategorizeTris event from Renderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_renderer_RecategorizeTris(object sender, EventArgs e)
        {
            if (Categories != null)
                RecategorizeSelectedTriangles();
        }

        /// <summary>
        /// Removes the currently selected triangles from their respective groups and puts them into a new group.
        /// </summary>
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

        /// <summary>
        /// Puts the selected triangles into a new Group inside a new Category.
        /// </summary>
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

        /// <summary>
        /// Removes a Group that has no triangles from its Category's Group list.
        /// </summary>
        /// <param name="group">Group to remove</param>
        private void RemoveDeadGroup(Group group)
        {
            group.GroupCategory.Groups.Remove(group);

            if (group.GroupCategory.Groups.Count == 0)
            {
                RemoveDeadCategory(group.GroupCategory);
            }

            m_renderer.RenderableObjs.Remove(group);
        }

        /// <summary>
        /// Removes a Category that has no groups from the Category list.
        /// </summary>
        /// <param name="cat">Category to remove</param>
        private void RemoveDeadCategory(Category cat)
        {
            Categories.Remove(cat);
        }
        #endregion

        #region Triangle Selection
        /// <summary>
        /// Handler for Triangle selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_renderer_SelectedTris(object sender, SelectTriangleEventArgs e)
        {
            if (!Input.GetKey(System.Windows.Forms.Keys.LControlKey))
            {
                foreach (Triangle tri in SelectedTriangles.SelectedItems)
                {
                    tri.IsSelected = false;
                }

                SelectedTriangles.SelectedItems.Clear();
            }

            if (e.SelectedTris.Count != 0)
            {
                foreach (Triangle tri in e.SelectedTris)
                {
                    if (SelectedTriangles.SelectedItems.Contains(tri))
                    {
                        tri.IsSelected = false;

                        SelectedTriangles.SelectedItems.Remove(tri);
                    }

                    else
                    {
                        tri.IsSelected = true;

                        SelectedTriangles.SelectedItems.Add(tri);
                    }
                }
            }


            PropGrid.Update();
        }

        /// <summary>
        /// Adds each Group to the list of renderable objects kept by the renderer.
        /// </summary>
        private void AddRenderableObjs()
        {
            foreach (Category cat in Categories)
            {
                foreach (Group grp in cat.Groups)
                    m_renderer.RenderableObjs.Add(grp);
            }
        }
        #endregion

        #region Nintendo-Specific
        /*
         * These are obsolete pending addition of WArchiveTools from LordNed and Gamma/Sage of Mirrors.
         * 
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
         */
        #endregion

        #region File Menu
        /// <summary>
        /// Asks if the user would like to save the currently loaded file and closes it if the user answers Yes or No.
        /// </summary>
        private void Close()
        {
            if (AskSaveFile())
            {
                SelectedTriangles.SelectedItems.Clear();

                SelectedTriangles.HasSelection = false;

                PropGrid.Update();

                Categories.Clear();

                m_renderer.RenderableObjs.Clear();

                isDataLoaded = false;

                m_undoRedoManager.Clear();

                CurrentFile = "";
            }
        }

        /// <summary>
        /// Asks if the user would like to save the currently loaded file
        /// and closes the application if the user answers Yes or No.
        /// </summary>
        private void ExitApplication()
        {
            if (AskSaveFile())
                Application.Current.MainWindow.Close();
        }
        #endregion

        #region Help Menu
        /// <summary>
        /// Displays the About window.
        /// </summary>
        private void DisplayAboutWindow()
        {
            AboutWindow win = new AboutWindow();

            win.Show();
        }

        /// <summary>
        /// Opens the user's default browser to DZBCollisionEditor's Issues page.
        /// </summary>
        private void ReportBug()
        {
            System.Diagnostics.Process.Start("https://github.com/Sage-of-Mirrors/DZBCollisionEditor/issues");
        }

        /// <summary>
        /// Opens the user's default browser to DZBCollisionEditor's Wiki page.
        /// </summary>
        private void OpenWiki()
        {
            System.Diagnostics.Process.Start("https://github.com/Sage-of-Mirrors/DZBCollisionEditor/wiki");
        }
        #endregion

        #region Command Callbacks
        /// <summary> The user has requested to open a new map, ask which map and then unload current if needed. </summary>
        public ICommand OnRequestMapOpen
        {
            get { return new RelayCommand(x => OpenFileFromDialog()); }
        }

        /// <summary> The user has requested to save the currently open map. Only available if a map is currently opened. </summary>
        public ICommand OnRequestMapSave
        {
            get { return new RelayCommand(x => Save(), x => isDataLoaded); }
        }

        /// <summary> The user has requested to save the currently open map in a new file. Only available if a map is currently opened. </summary>
        public ICommand OnRequestMapSaveAs
        {
            get { return new RelayCommand(x => SaveAs(), x => isDataLoaded); }
        }

        /// <summary> The user has requested to unload the currently open map. Only available if a map is currently opened. Ask user if they'd like to save. </summary>
        public ICommand OnRequestMapClose
        {
            get { return new RelayCommand(x => Close(), x => isDataLoaded); }
        }

        /// <summary> The user has requested to undo the last action. Only available if they've made an undoable action. </summary>
        public ICommand OnRequestUndo
        {
            get { return new RelayCommand(x => m_undoRedoManager.Undo(), x => m_undoRedoManager.CanUndo()); }
        }

        /// <summary> The user has requested to redo the last undo action. Only available if they've undone an action. </summary>
        public ICommand OnRequestRedo
        {
            get { return new RelayCommand(x => m_undoRedoManager.Redo(), x => m_undoRedoManager.CanRedo()); }
        }

        /// <summary> The user has requested to regroup the currently selected Triangles. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestRegroupTris
        {
            get { return new RelayCommand(x => RegroupSelectedTriangles(), x => SelectedTriangles.SelectedItems.Count != 0); }
        }

        /// <summary> The user has requested to recategorize the currently selected Triangles. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestRecategorizeTris
        {
            get { return new RelayCommand(x => RecategorizeSelectedTriangles(), x => SelectedTriangles.SelectedItems.Count != 0); }
        }

        /// <summary> The user has pressed Alt + F4, chosen Exit from the File menu, or clicked the close button. </summary>
        public ICommand OnRequestApplicationClose
        {
            get { return new RelayCommand(x => ExitApplication()); }
        }

        /// <summary> The user has clicked About from the Help menu. </summary>
        public ICommand OnRequestDisplayAbout
        {
            get { return new RelayCommand(x => DisplayAboutWindow()); }
        }

        /// <summary> The user has clicked Report a Bug... from the Help menu. </summary>
        public ICommand OnRequestReportBug
        {
            get { return new RelayCommand(x => ReportBug()); }
        }

        /// <summary> The user has clicked Wiki from the Help menu. </summary>
        public ICommand OnRequestOpenWiki
        {
            get { return new RelayCommand(x => OpenWiki()); }
        }
        #endregion

        #region Misc
        /// <summary>
        /// Returns an IEnumerable<TerrainType> collection used to populate the TerrainType selector on the main window.
        /// </summary>
        public IEnumerable<TerrainType> TerrainTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(TerrainType))
                    .Cast<TerrainType>();
            }
        }

        /// <summary>
        /// Returns an IEnumerable<TerrainType> collection used to populate the TerrainType selector on the main window.
        /// </summary>
        public IEnumerable<TriSelectionType> TriSelectionTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(TriSelectionType))
                    .Cast<TriSelectionType>();
            }
        }
        #endregion
    }
}
