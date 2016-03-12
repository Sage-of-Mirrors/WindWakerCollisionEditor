using System;
using System.Collections.Generic;
using System.Linq;
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
using OpenTK;
using OpenTK.Graphics;

namespace WindWakerCollisionEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel m_viewModel;

        public MainWindow()
        {
            m_viewModel = new ViewModel();

            InitializeComponent();

            m_viewModel.SetTreeView(treeView);

            m_viewModel.test = testPropGrid;

            DataContext = m_viewModel;
        }

        private void GLHost_Initialized(object sender, EventArgs e)
        {
            GLControl m_glControl;

            m_glControl = new GLControl(new GraphicsMode(32,24), 3, 0, GraphicsContextFlags.Default);
            m_glControl.MakeCurrent();
            m_glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            m_glControl.AllowDrop = true;
            m_glControl.BackColor = System.Drawing.Color.Fuchsia;
            m_viewModel.CreateGraphicsContext(m_glControl, GLHost, testPropGrid, RecentFileList);

            GLHost.Child = m_glControl;
            GLHost.AllowDrop = true;
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
