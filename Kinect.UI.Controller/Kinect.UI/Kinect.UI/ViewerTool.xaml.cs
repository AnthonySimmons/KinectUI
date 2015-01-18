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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using Fizbin.Kinect.Gestures;
using Fizbin.Kinect.Gestures.Segments;
using System.Timers;
using System.ComponentModel;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect.Toolkit;

using System.Runtime.InteropServices;
using System.Windows.Navigation; 

namespace Kinect.UI
{
    /// <summary>
    /// Interaction logic for ViewerTool.xaml
    /// </summary>
    public partial class ViewerTool : Window, INotifyPropertyChanged
    {
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_SCROLL = 0x800;

        JointType cursorJoint = JointType.HandRight;

        public double CursorBoxScale = .9;

        DrawingGroup drawingGroup = new DrawingGroup();
        byte[] colorPixels;
        WriteableBitmap imgSource;
        DrawingImage dI;
       
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        private Skeleton[] skeletons = new Skeleton[0];

        System.Timers.Timer _clearTimer;

        // skeleton gesture recognizer
        private GestureController gestureController;

        IController w8;

        bool swipeCheck = true;
        System.Timers.Timer swipeTimer = new System.Timers.Timer();
        
        public ViewerTool()
        {
            InitializeComponent();

            this.w8 = new Windows8();
            this.swipeTimer.Elapsed += swipeTimer_Elapsed;
            this.swipeTimer.Interval = 1500;

            var desktopWorkingArea = Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            DataContext = this;

            InitializeComponent();

            // initialize the Kinect sensor manager
            KinectSensorManager = new KinectSensorManager();
            KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;

            // locate an available sensor
            sensorChooser.Start();

            // bind chooser's sensor value to the local sensor manager
            var kinectSensorBinding = new System.Windows.Data.Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

			// add timer for clearing last detected gesture
            _clearTimer = new System.Timers.Timer(2000);
            //_clearTimer.Elapsed += new ElapsedEventHandler(clearTimer_Elapsed);

            drawingGroup = new DrawingGroup();

            dI = new DrawingImage(this.drawingGroup);
            img2.Source = dI;
        }

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
                UninitializeKinectServices(args.OldValue);

            if (null != args.NewValue)
                InitializeKinectServices(KinectSensorManager, args.NewValue);
        }

        /// <summary>
        /// Kinect enabled apps should customize which Kinect services it initializes here.
        /// </summary>
        /// <param name="kinectSensorManager"></param>
        /// <param name="sensor"></param>
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.

            // configure the color stream
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            // configure the depth stream
            kinectSensorManager.DepthStreamEnabled = true;

            kinectSensorManager.TransformSmoothParameters =
                new TransformSmoothParameters
                {
                    Smoothing = 0.7f,          //.5
                    Correction = 0.3f,         //.5
                    Prediction = 1.0f,         //.5
                    JitterRadius = 0.05f,      //.5
                    MaxDeviationRadius = 1.0f  //.4
                };

            // configure the skeleton stream
            //sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            kinectSensorManager.SkeletonStreamEnabled = true;
            
            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // initialize the gesture recognizer
                gestureController = new GestureController();
                gestureController.GestureRecognized += OnGestureRecognized;

                // register the gestures for this demo
                RegisterGestures();
            }

            kinectSensorManager.ElevationAngle = (int)10;
        }

        /// <summary>
        /// Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        /// </summary>
        /// <param name="sensor"></param>
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            // unregister the event handlers
            sensor.SkeletonFrameReady -= OnSkeletonFrameReady;
            gestureController.GestureRecognized -= OnGestureRecognized;
        }

        /// <summary>
        /// Helper function to register all available 
        /// </summary>
        private void RegisterGestures()
        {
            // define the gestures for the demo
            IRelativeGestureSegment[] mouseClick = new IRelativeGestureSegment[2];
            mouseClick[0] = new MouseClick();
            mouseClick[1] = new MouseClick2();
            gestureController.AddGesture("MouseClick", mouseClick);

            IRelativeGestureSegment[] mouseDownSegments = new IRelativeGestureSegment[2];
            mouseDownSegments[0] = new MouseDown();
            mouseDownSegments[1] = new MouseDown2();
            gestureController.AddGesture("MouseDown", mouseDownSegments);

            IRelativeGestureSegment[] mouseUpSegments = new IRelativeGestureSegment[2];
            mouseUpSegments[0] = new MouseUp();
            mouseUpSegments[1] = new MouseUp2();
            gestureController.AddGesture("MouseUp", mouseUpSegments);

            IRelativeGestureSegment[] handtoear = new IRelativeGestureSegment[50];
            for (int i = 0; i < handtoear.Count(); i++)
            {
                handtoear[i] = new HandToEar();
            }
            gestureController.AddGesture("HandtoEar", handtoear);
            

            IRelativeGestureSegment[] joinedhandsSegments = new IRelativeGestureSegment[20];
            JoinedHandsSegment1 joinedhandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 10 times 
                joinedhandsSegments[i] = joinedhandsSegment;
            }
            gestureController.AddGesture("JoinedHands", joinedhandsSegments);

            IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[20];
            MenuSegment1 menuSegment = new MenuSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 20 times 
                menuSegments[i] = menuSegment;
            }
            gestureController.AddGesture("Menu", menuSegments);

            IRelativeGestureSegment[] swipeleftSegments = new IRelativeGestureSegment[3];
            swipeleftSegments[0] = new SwipeLeftSegment1();
            swipeleftSegments[1] = new SwipeLeftSegment2();
            swipeleftSegments[2] = new SwipeLeftSegment3();
            gestureController.AddGesture("SwipeLeft", swipeleftSegments);

            IRelativeGestureSegment[] swiperightSegments = new IRelativeGestureSegment[3];
            swiperightSegments[0] = new SwipeRightSegment1();
            swiperightSegments[1] = new SwipeRightSegment2();
            swiperightSegments[2] = new SwipeRightSegment3();
            gestureController.AddGesture("SwipeRight", swiperightSegments);

            IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[6];
            WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
            WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
            waveRightSegments[0] = waveRightSegment1;
            waveRightSegments[1] = waveRightSegment2;
            waveRightSegments[2] = waveRightSegment1;
            waveRightSegments[3] = waveRightSegment2;
            waveRightSegments[4] = waveRightSegment1;
            waveRightSegments[5] = waveRightSegment2;
            gestureController.AddGesture("WaveRight", waveRightSegments);

            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[5];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            waveLeftSegments[4] = waveLeftSegment1;
            //waveLeftSegments[5] = waveLeftSegment2;
            gestureController.AddGesture("WaveLeft", waveLeftSegments);

            IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            zoomInSegments[0] = new ZoomSegment1();
            zoomInSegments[1] = new ZoomSegment2();
            zoomInSegments[2] = new ZoomSegment3();
            gestureController.AddGesture("ZoomIn", zoomInSegments);

            IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            zoomOutSegments[0] = new ZoomSegment3();
            zoomOutSegments[1] = new ZoomSegment2();
            zoomOutSegments[2] = new ZoomSegment1();
            gestureController.AddGesture("ZoomOut", zoomOutSegments);

            IRelativeGestureSegment[] swipeUpSegments = new IRelativeGestureSegment[3];
            swipeUpSegments[0] = new SwipeUpSegment1();
            swipeUpSegments[1] = new SwipeUpSegment2();
            swipeUpSegments[2] = new SwipeUpSegment3();
            gestureController.AddGesture("SwipeUp", swipeUpSegments);

            IRelativeGestureSegment[] swipeDownSegments = new IRelativeGestureSegment[3];
            swipeDownSegments[0] = new SwipeDownSegment1();
            swipeDownSegments[1] = new SwipeDownSegment2();
            swipeDownSegments[2] = new SwipeDownSegment3();
            gestureController.AddGesture("SwipeDown", swipeDownSegments);
        }

        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the last recognized gesture.
        /// </summary>
        private string _gesture;
        public String Gesture
        {
            get { return _gesture; }

            private set
            {
                if (_gesture == value)
                    return;

                _gesture = value;

                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            
            switch (e.GestureName)
            {
                case "MouseClick":
                    System.Media.SystemSounds.Beep.Play();
                    Gesture = "Mouse Click";
                    w8.DoMouseLeftClick();
                    break;
                case "MouseDown":
                    System.Media.SystemSounds.Beep.Play();
                    Gesture = "Mouse Down";
                    w8.DoMouseLeftClickDown();
                    break;
                case "MouseUp":
                    System.Media.SystemSounds.Asterisk.Play();
                    Gesture = "Mouse Up";
                    w8.DoMouseLeftClickUp();
                    break;
                case "Menu":
                    Gesture = "Menu";
                    break;
                case "WaveRight":
                    Gesture = "Wave Right";
                    break;
                case "WaveLeft":
                    Gesture = "Wave Left";
                    w8.OpenStartMenu();
                    break;
                case "JoinedHands":
                    Gesture = "Joined Hands";
                    break;
                case "SwipeLeft":
                    if (swipeCheck)
                    {
                        Gesture = "Swipe Left";
                        w8.ChangeTabBackward();
                        this.swipeCheck = false;
                        this.swipeTimer.Start();
                    }
                    break;
                case "SwipeRight":
                    if (swipeCheck)
                    {
                        Gesture = "Swipe Right";
                        w8.ChangeTabForward();
                        this.swipeCheck = false;
                        this.swipeTimer.Start();
                    }
 					break;
                case "SwipeUp":
                    Gesture = "Swipe Up";
                    break;
                case "SwipeDown":
                    Gesture = "Swipe Down";
                    break;
                case "ZoomIn":
                    Gesture = "Zoom In";
                    w8.ZoomIn();
                    break;
                case "ZoomOut":
                    Gesture = "Zoom Out";
                    w8.ZoomOut();
                    break;
                case "HandtoEar":
                    Gesture = "HandtoEar";
                    break;
                default:
                    break;
            }
            gestureController.clearAllProgress();
        }

        private void swipeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.swipeTimer.Stop();
            this.swipeCheck = true;
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                // resize the skeletons array if needed
                if (skeletons.Length != frame.SkeletonArrayLength)
                    skeletons = new Skeleton[frame.SkeletonArrayLength];

                // get the skeleton data
                frame.CopySkeletonDataTo(skeletons);
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, dI.Width, dI.Height));
                    foreach (var skeleton in skeletons)
                    {
                        // skip the skeleton if it is not being tracked
                        if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                            continue;

                        // update the gesture controller
                        gestureController.UpdateAllGestures(skeleton);
                        drawCursorBox(skeleton, dc);
                    }
                }
            }
        }

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
        private Point skeletonPointToScreen(SkeletonPoint pt)
        {
            DepthImagePoint dp = KinectSensorManager.KinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, DepthImageFormat.Resolution640x480Fps30);
            Point wPt = new Point();
            wPt.X = dp.X;
            wPt.Y = dp.Y;
            return wPt;
        }
        private double screenPointDis(Point pt1, Point pt2)
        {
            return (Math.Sqrt(Math.Pow((pt1.X - pt2.X), 2) + Math.Pow((pt1.Y - pt2.Y), 2)));
        }
        private bool boxContains(int x, int y, int wid, int het, Point pt)
        {
            return (pt.X > x && pt.Y > y && pt.X < x + wid && pt.Y < y + het);
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
            Point topLeft, topRight, botLeft, botRight;
            
            Point hipCenter = skeletonPointToScreen(skel.Joints[JointType.HipCenter].Position);
            Point chestCenter = skeletonPointToScreen(skel.Joints[JointType.ShoulderCenter].Position);

            double het = screenPointDis(hipCenter, chestCenter) * this.CursorBoxScale;

            double ratio = (double)Screen.PrimaryScreen.Bounds.Width /  (double)Screen.PrimaryScreen.Bounds.Height;
            double wid = het * ratio;

            topLeft = chestCenter;
            topLeft.X += 50;

            topRight = topLeft;
            topRight.X += wid;

            botLeft = topLeft;
            botLeft.Y -= het;
            botRight = topRight;
            botRight.Y -= het;

            //drawBox(dc, topLeft, topRight, botLeft, botRight);
            doHandCursor(skel, (int)topLeft.X, (int)topLeft.Y, (int)wid, (int)het);
        }
        private void drawBox(DrawingContext dc, Point topLeft, Point topRight, Point botLeft, Point botRight)
        {
            Pen pen = new Pen(Brushes.White, 3);
            dc.DrawLine(pen, topLeft, topRight);
            dc.DrawLine(pen, topLeft, botLeft);
            dc.DrawLine(pen, topRight, botRight);
            dc.DrawLine(pen, botLeft, botRight);
        }
        
        DoubleAnimation da;
        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var desktopWorkingArea = Screen.PrimaryScreen.WorkingArea;
            if (this.Top == (desktopWorkingArea.Top + 10))
            {
                da = new DoubleAnimation() { From = this.Top, To = desktopWorkingArea.Bottom - this.Height - 10, Duration = TimeSpan.FromMilliseconds(250) };
            }
            else
            {
                da = new DoubleAnimation() { From = this.Top, To = desktopWorkingArea.Top + 10, Duration = TimeSpan.FromMilliseconds(250) };
            }
            BeginAnimation(Window.TopProperty, da);
        }
    }
}