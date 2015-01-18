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
using System.Drawing;
using System.Windows.Forms;

namespace Kinect.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewerTool viewer;

        NotifyIcon nIcon = new NotifyIcon();
        ContextMenuStrip nIconMenu = new ContextMenuStrip();
        ToolStripMenuItem showAppToolStripMenuItem = new ToolStripMenuItem("show");
        ToolStripMenuItem hideAppToolStripMenuItem = new ToolStripMenuItem("hide");
        ToolStripMenuItem quitApp = new ToolStripMenuItem("quit");


        public bool Capture { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            initNotifyIcon();

            viewer = new ViewerTool();
            viewer.Show();
        }

        ~MainWindow()
        {
            this.nIcon.Icon = null;
            this.nIcon.Dispose();
        }

        private void initNotifyIcon()
        {
            this.nIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAppToolStripMenuItem,
            this.hideAppToolStripMenuItem,
            this.quitApp});
            quitApp.MouseDown += quitApplication;
            this.nIconMenu.Name = "contextMenuStrip1";
            this.nIconMenu.Size = new System.Drawing.Size(153, 70);

            this.nIcon.Icon = new Icon(@"../../favicon.ico");
            this.nIcon.ContextMenuStrip = this.nIconMenu;
            this.nIcon.Visible = true;

            this.nIcon.BalloonTipTitle = "Flip a Bit";
            this.nIcon.BalloonTipText = "System Intializing...";
            this.nIcon.ShowBalloonTip(3000);
        }

        private void quitApplication(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
