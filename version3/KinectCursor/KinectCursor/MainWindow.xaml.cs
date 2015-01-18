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
using Microsoft.Kinect;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace KinectCursor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
   
    public partial class MainWindow : Window
    {

        KinectSensor sensor;
        DrawingGroup drawingGroup = new DrawingGroup();
        byte[] colorPixels;
        WriteableBitmap imgSource;
        DrawingImage dI;
        JointType cursorJoint = JointType.HandRight;

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_SCROLL = 0x800;

        private void doMouseScroll(bool up)
        {
            if (up)
            {
                mouse_event(MOUSEEVENTF_SCROLL, 0, 0, 120, 0);
            }
            else
            {
                mouse_event(MOUSEEVENTF_SCROLL, 0, 0, unchecked((uint)-120), 0);
            }
        }

        private void doMouseClick(uint mouseEvent)
        { 
            uint x = (uint)System.Windows.Forms.Cursor.Position.X;
            uint y = (uint)System.Windows.Forms.Cursor.Position.Y;
            mouse_event(mouseEvent, x, y, 0, 0);
        }


        public void connectKinect()
        {
            foreach (KinectSensor ks in KinectSensor.KinectSensors)
            {
                if (ks.Status == KinectStatus.Connected)
                {
                    sensor = ks;
                    
                }
            }
        }

        private Point skeletonPointToScreen(SkeletonPoint pt)
        {
            DepthImagePoint dp = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, DepthImageFormat.Resolution640x480Fps30);
            Point wPt = new Point();
            wPt.X = dp.X;
            wPt.Y = dp.Y;
            return wPt;
        }
        private double screenPointDis(Point pt1, Point pt2)
        { 
            return (Math.Sqrt( Math.Pow((pt1.X-pt2.X), 2) + Math.Pow((pt1.Y-pt2.Y), 2)) );
        }

        private bool boxContains(int x, int y, int wid, int het, Point pt)
        { 
            return (pt.X > x && pt.Y > y && pt.X < x+wid && pt.Y < y+het);
        }

        private Point avgScreenPoint(Point pt1, Point pt2)
        { 
            return (new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2));
        }

        private Point lastHand = new Point();
        private void doHandCursor(Skeleton skel, int boxX, int boxY, int boxWid, int boxHet)
        {
            if (skel.Joints[cursorJoint].TrackingState == JointTrackingState.Tracked)
            {
                Point hand = this.skeletonPointToScreen(skel.Joints[cursorJoint].Position);
                if (boxWid > 0 && boxHet > 0)
                {
                    double ratioWid = Screen.PrimaryScreen.Bounds.Width / boxWid;
                    double ratioHet = Screen.PrimaryScreen.Bounds.Height / boxHet;

                    hand.X -= boxX;
                    hand.Y -= boxY;

                    hand.X *= ratioWid;
                    hand.Y *= ratioHet;

                    Point avg = avgScreenPoint(lastHand, hand);
                    lastHand = avg;
                    SetCursorPos((int)hand.X, (int)hand.Y);
                }
            }
        }

        private void drawCursorBox(Skeleton skel, DrawingContext dc)
        {
            //if (skel.TrackingState == SkeletonTrackingState.Tracked)
            {
                img2.Margin = img.Margin;
                img2.Width = img.Width;
                img2.Height = img.Height;
                Point head = skeletonPointToScreen(skel.Joints[JointType.Head].Position);
                Point center = skeletonPointToScreen(skel.Joints[JointType.ShoulderCenter].Position);
                Point hip = skeletonPointToScreen(skel.Joints[JointType.HipCenter].Position);
                Point left = skeletonPointToScreen(skel.Joints[JointType.ShoulderLeft].Position);
                Point right = skeletonPointToScreen(skel.Joints[JointType.ShoulderRight].Position);

                double het = screenPointDis(head, hip);

                double ratio = (double)(Screen.PrimaryScreen.Bounds.Width / Screen.PrimaryScreen.Bounds.Height);
                double wid = het / ratio;

                head.X += 50;
                

                Point topLeft, topRight, botLeft, botRight;

                topRight = head;
                topRight.X += 50;

                botRight = topRight;
                botRight.Y += het;

                topLeft = topRight;
                topLeft.X -= wid;

                botLeft = botRight;
                botLeft.X -= wid;


                drawBox(dc, topLeft, topRight, botLeft, botRight);
                doHandCursor(skel, (int)topLeft.X, (int)topLeft.Y, (int)wid, (int)het);
            }
        }
        private void drawBox(DrawingContext dc, Point topLeft, Point topRight, Point botLeft, Point botRight)
        {
            img2.Margin = img.Margin;
            img2.Width = img.Width;
            img2.Height = img.Height;
            Pen pen = new Pen(Brushes.Blue, 3);
            dc.DrawLine(pen, topLeft, topRight);
            dc.DrawLine(pen, topLeft, botLeft);
            dc.DrawLine(pen, topRight, botRight);
            dc.DrawLine(pen, botLeft, botRight);
        }

        private void drawSkeleton(Skeleton skel, DrawingContext dc)
        {
            img2.Margin = img.Margin;
            img2.Width = img.Width;
            img2.Height = img.Height;
            foreach (Joint joint in skel.Joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    System.Windows.Point pt = this.skeletonPointToScreen(skel.Joints[joint.JointType].Position);
                    if (joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight)
                    {
                        dc.DrawEllipse(Brushes.Red, null, pt, 3, 3);
                    }
                    else
                    {
                        dc.DrawEllipse(Brushes.Yellow, null, pt, 3, 3);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            img2.Margin = img.Margin;
            img2.Width = img.Width;
            img2.Height = img.Height;

            drawingGroup = new DrawingGroup();
            connectKinect();
            if (sensor != null)
            {
                sensor.Start();
            }
            else
            {
                return;
            }

            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
            sensor.ColorFrameReady += sensor_ColorFrameReady;
            sensor.DepthFrameReady += sensor_DepthFrameReady;
            TransformSmoothParameters trans = new TransformSmoothParameters();
            //Minimum
            /*trans.Smoothing = 0.5f;
            trans.Correction = 0.5f;
            trans.Prediction = 0.5f;
            trans.JitterRadius = 0.05f;
            trans.MaxDeviationRadius = 0.04f;
            
            //Medium
            trans.Smoothing = 0.5f;
            trans.Correction = 0.1f;
            trans.Prediction = 0.5f;
            trans.JitterRadius = 0.1f;
            trans.MaxDeviationRadius = 0.1f;
            */

            //Maximum
            trans.Smoothing = 0.7f;
            trans.Correction = 0.3f;
            trans.Prediction = 1.0f;
            trans.JitterRadius = 1.0f;
            trans.MaxDeviationRadius = 1.0f;

            //sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            //sensor.DepthStream.Range = DepthRange.Near;

            sensor.ElevationAngle = 0;
            sensor.SkeletonStream.Enable(trans);
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            
            colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];

            dI = new DrawingImage(this.drawingGroup);
            imgSource = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            img.Source = imgSource;
            img2.Source = dI;
            

        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using(DepthImageFrame df = e.OpenDepthImageFrame())
            {
                
                if (df != null)
                { 
                      
                }
            }
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame cf = e.OpenColorImageFrame())
            {
                if (cf != null)
                {
                    cf.CopyPixelDataTo(colorPixels);
                }
            }
            this.imgSource.WritePixels(
        new Int32Rect(0, 0, this.imgSource.PixelWidth, this.imgSource.PixelHeight),
        this.colorPixels,
        this.imgSource.PixelWidth * sizeof(int),
        0);
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton [] skel = new Skeleton[0];
            using(SkeletonFrame sf = e.OpenSkeletonFrame())
            {
                if (sf != null)
                {
                       
                    skel = new Skeleton[sf.SkeletonArrayLength];
                    sf.CopySkeletonDataTo(skel);
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, dI.Width, dI.Height));
                        foreach(Skeleton sk in skel)
                        {
                            if (sk.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                drawCursorBox(sk, dc);
                                drawSkeleton(sk, dc);
                                break;
                            }
                        }
                    }
                }
            }
        }



    }
}
