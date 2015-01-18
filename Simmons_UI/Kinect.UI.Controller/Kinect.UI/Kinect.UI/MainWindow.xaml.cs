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
using System.Threading;


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
        Thread audioThread;



        bool dragWindow = false;
        public bool Capture { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            initNotifyIcon();
            sliderTransparency.Minimum = 0;
            sliderTransparency.Maximum = 100;
            sliderSize.Minimum = 0;
            sliderSize.Maximum = 100;
            viewer = new ViewerTool();
            viewer.Show();
            this.Hide();
            lblTitle.Content = "Kinect Cursor Control \n Flip-A-Bit Inc.";


        }

        ~MainWindow()
        {
            this.nIcon.Icon = null;
            this.nIcon.Dispose();
        }

        private void initNotifyIcon()
        {
            Font font = new Font("Times New Roman", 12);
            ToolStripMenuItem showAppToolStripMenuItem = new ToolStripMenuItem("Show");
            showAppToolStripMenuItem.Click += showAppToolStripMenuItem_Click;
            ToolStripMenuItem hideAppToolStripMenuItem = new ToolStripMenuItem("Hide");
            hideAppToolStripMenuItem.Click += hideAppToolStripMenuItem_Click;
            ToolStripMenuItem quitApp = new ToolStripMenuItem("Quit");
            quitApp.Click += quitApp_Click;
            ToolStripMenuItem settings = new ToolStripMenuItem("Settings");
            settings.Click += settings_Click;

            showAppToolStripMenuItem.CheckOnClick = true;
            hideAppToolStripMenuItem.CheckOnClick = true;
            quitApp.CheckOnClick = true;
            settings.CheckOnClick = true;

            showAppToolStripMenuItem.Font = font;
            hideAppToolStripMenuItem.Font = font;
            quitApp.Font = font;
            settings.Font = font;

            

            
            this.nIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            showAppToolStripMenuItem,
            hideAppToolStripMenuItem,
            quitApp,
            settings});
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

        void settings_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        void quitApp_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void hideAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.Hide();
        }

        void showAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.Show();
        }

        private void quitApplication(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void sliderTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewer.Opacity = sliderTransparency.Value / 100;
        }

        private void sliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            int het = Screen.PrimaryScreen.WorkingArea.Height; 
            viewer.Width = (this.sliderSize.Value  * wid) / 100f;
            viewer.Height = (sliderSize.Value * het) / 100f;

            viewer.Left = wid - viewer.Width;
            viewer.Top = het - viewer.Height;
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (btnDisable.Content.ToString() == "Disable Window")
            {
                viewer.Hide();
                btnDisable.Content = "Enable Window";
            }
            else if (btnDisable.Content.ToString() == "Enable Window")
            {
                viewer.Show();
                btnDisable.Content = "Disable Window";
            }
        }

        private void sliderBoxSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void rdoMinSmooth_Checked(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Kinect.TransformSmoothParameters trans = new Microsoft.Kinect.TransformSmoothParameters();

            //Minimum
            trans.Smoothing = 0.5f;
            trans.Correction = 0.5f;
            trans.Prediction = 0.5f;
            trans.JitterRadius = 0.05f;
            trans.MaxDeviationRadius = 0.04f;
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Disable(); 
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Enable(trans);
        }

        private void rdoMedSmooth_Checked(object sender, RoutedEventArgs e)
        {
            Microsoft.Kinect.TransformSmoothParameters trans = new Microsoft.Kinect.TransformSmoothParameters();
            //Medium
            trans.Smoothing = 0.5f;
            trans.Correction = 0.1f;
            trans.Prediction = 0.5f;
            trans.JitterRadius = 0.1f;
            trans.MaxDeviationRadius = 0.1f;
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Disable();
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Enable(trans);

        }

        private void rdoMaxSmooth_Checked(object sender, RoutedEventArgs e)
        {
            Microsoft.Kinect.TransformSmoothParameters trans = new Microsoft.Kinect.TransformSmoothParameters();
            trans.Smoothing = 0.7f;
            trans.Correction = 0.3f;
            trans.Prediction = 1.0f;
            trans.JitterRadius = 1.0f;
            trans.MaxDeviationRadius = 1.0f;
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Disable();
            viewer.KinectSensorManager.KinectSensor.SkeletonStream.Enable(trans);
        }

        
        private void btnDisableGestures_Click_1(object sender, RoutedEventArgs e)
        {
            if (btnDisableGestures.Content.ToString() == "Disable Gestures")
            {
                btnDisableGestures.Content = "Enable Gestures";
                viewer.enableGestures = false;
            }
            else if (btnDisableGestures.Content.ToString() == "Enable Gestures")
            {
                btnDisableGestures.Content = "Disable Gestures";
                viewer.enableGestures = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

   
    }
}
