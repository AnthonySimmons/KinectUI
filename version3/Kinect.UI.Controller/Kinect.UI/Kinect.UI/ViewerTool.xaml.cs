using System;
using System.Collections.Generic;
using System.IO;
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
using System.Threading;
using System.ComponentModel;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect.Toolkit;
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Speech.Synthesis;
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
        public bool enableGestures = true;

        IController w8;

        bool swipeCheck = true;
        System.Timers.Timer swipeTimer = new System.Timers.Timer();


        bool checkVolume = false;
        System.Timers.Timer volumeTimer = new System.Timers.Timer();
        
        public ViewerTool()
        {
            InitializeComponent();

            this.w8 = new Windows8();
            this.swipeTimer.Elapsed += swipeTimer_Elapsed;
            this.swipeTimer.Interval = 1500;

            this.volumeTimer.Elapsed += volumeTimer_Elapsed;
            this.volumeTimer.Interval = 1000;

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
            img2.Height = this.Height;
            img2.Width = this.Width;            
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

            audioStream = sensor.AudioSource.Start();
            audioThread = new Thread(audioReadingThread);
            audioReading = true;
            speechEngine.SpeechRecognized += speechEngine_SpeechRecognized;
            speechEngine.SpeechDetected += speechEngine_SpeechDetected;
            loadDictionary();
            speechEngine.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            
            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // initialize the gesture recognizer
                gestureController = new GestureController();
                //gestureController.GestureRecognized += OnGestureRecognized;

                // register the gestures for this demo
                //RegisterGestures();
            }

            kinectSensorManager.ElevationAngle = (int)10;
        }

        private void audioReadingThread()
        {
            while (this.audioReading)
            { }
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

            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[4];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            gestureController.AddGesture("WaveLeft", waveLeftSegments);

            IRelativeGestureSegment[] scrolldownsegments = new IRelativeGestureSegment[4];
            scrolldownsegments[0] = new ScrollDown1();
            scrolldownsegments[1] = new ScrollDown2();
            scrolldownsegments[2] = new ScrollDown3();
            scrolldownsegments[3] = new ScrollDown4();
            gestureController.AddGesture("ScrollDown", scrolldownsegments);

            IRelativeGestureSegment[] scrollupsegments = new IRelativeGestureSegment[4];
            scrollupsegments[0] = new ScrollUp1();
            scrollupsegments[1] = new ScrollUp2();
            scrollupsegments[2] = new ScrollUp3();
            scrollupsegments[3] = new ScrollUp4();
            gestureController.AddGesture("ScrollUp", scrollupsegments);

            /*IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            zoomInSegments[0] = new ZoomSegment1();
            zoomInSegments[1] = new ZoomSegment2();
            zoomInSegments[2] = new ZoomSegment3();
            gestureController.AddGesture("ZoomIn", zoomInSegments);

            IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            zoomOutSegments[0] = new ZoomSegment3();
            zoomOutSegments[1] = new ZoomSegment2();
            zoomOutSegments[2] = new ZoomSegment1();
            gestureController.AddGesture("ZoomOut", zoomOutSegments);*/

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

            IRelativeGestureSegment[] volumeIncrease = new IRelativeGestureSegment[15];
            for (int i = 0; i < volumeIncrease.Count(); i++)
            {
                volumeIncrease[i] = new volumeUp();
            }
            gestureController.AddGesture("VolumeUp", volumeIncrease);

            IRelativeGestureSegment[] volumeDecrease = new IRelativeGestureSegment[15];
            for (int i = 0; i < volumeDecrease.Count(); i++)
            {
                volumeDecrease[i] = new volumeDown();
            }
            gestureController.AddGesture("VolumeDown", volumeDecrease);
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
            SetGestureLabel(e.GestureName);
            switch (e.GestureName)
            {
                case "MouseClick":
                    System.Media.SystemSounds.Asterisk.Play();
                    Gesture = "Mouse Click";
                    w8.DoMouseLeftClick();
                    break;
                case "MouseDown":
                    System.Media.SystemSounds.Asterisk.Play();
                    Gesture = "Mouse Down";
                    w8.DoMouseLeftClickDown();
                    break;
                case "MouseUp":
                    System.Media.SystemSounds.Asterisk.Play();
                    Gesture = "Mouse Up";
                    w8.DoMouseLeftClickUp();
                    break;
                case "ScrollDown":
                    Gesture = "Scroll Down";
                    w8.DoMouseScroll(false);
                    break;
                case "ScrollUp":
                    Gesture = "Scroll Up";
                    w8.DoMouseScroll(true);
                    break;
                case "WaveLeft":
                    Gesture = "Wave Left";
                    w8.OpenStartMenu();
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
                case "HandtoEar":
                    System.Media.SystemSounds.Beep.Play();
                    w8.VolumeUp();
                    Gesture = "HandtoEar";
                    this.volumeTimer.Start();
                    this.checkVolume = true;
                    break;
                case "VolumeUp":
                    Gesture = "Volume Up";
                    w8.VolumeUp();
                    this.volumeTimer.Stop();
                    this.volumeTimer.Start();
                    break;
                case "VolumeDown":
                    Gesture = "Volume Down";
                    w8.VolumeDown();
                    this.volumeTimer.Stop();
                    this.volumeTimer.Start();
                    break;
                default:
                    return;
                    break;
            }
            gestureController.clearAllProgress();
        }

        private void swipeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.swipeTimer.Stop();
            this.swipeCheck = true;
        }

        private void volumeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.volumeTimer.Stop();
            this.checkVolume = false;
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
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, dI.Width, dI.Height));
                    foreach (var skeleton in skeletons)
                    {
                        // skip the skeleton if it is not being tracked
                        if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                            continue;

                        img2.Height = this.Height;
                        img2.Width = this.Width;

                        // update the gesture controller
                        //if(enableGestures)
                        //    gestureController.UpdateAllGestures(skeleton);
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
                    lastHand = hand;
                    SetCursorPos((int)avg.X, (int)avg.Y);
                }
            }
        }

        int frameCounter = 0;

        private void drawCursorBox(Skeleton skel, DrawingContext dc)
        {
            Point topLeft, topRight, botLeft, botRight;
            
            Point hipCenter = skeletonPointToScreen(skel.Joints[JointType.HipCenter].Position);
            Point chestCenter = skeletonPointToScreen(skel.Joints[JointType.ShoulderCenter].Position);

            double het = screenPointDis(hipCenter, chestCenter) * this.CursorBoxScale;

            double ratio = (double)Screen.PrimaryScreen.Bounds.Width /  (double)Screen.PrimaryScreen.Bounds.Height;
            double wid = het * ratio;

            topLeft = chestCenter;
            //topLeft.X += 50;

            topRight = topLeft;
            //topRight.X += wid;

            botLeft = topLeft;
            //botLeft.Y -= het;
            botRight = topRight;
            //botRight.Y -= het;

            drawBox(dc, topLeft, topRight, botLeft, botRight);

            if (frameCounter % 2 == 0)
            {
                doHandCursor(skel, (int)topLeft.X, (int)topLeft.Y, (int)wid, (int)het);
                frameCounter = 0;
                
            }
            frameCounter++;
        }
        private void drawBox(DrawingContext dc, Point topLeft, Point topRight, Point botLeft, Point botRight)
        {
            Pen pen = new Pen(Brushes.White, 3);
            dc.DrawLine(pen, topLeft, topRight);
            dc.DrawLine(pen, topLeft, botLeft);
            dc.DrawLine(pen, topRight, botRight);
            dc.DrawLine(pen, botLeft, botRight);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.grid1.Width = this.Width - 10;
            this.grid1.Height = this.Height - 10;
            this.grid0.Width = this.Width - 10;
            this.grid0.Height = this.Height - 10;
            this.DepthViewer.Width = this.Width - 10;
            this.DepthViewer.Height = this.Height - 10;

        }

        private void KinectSkeletonViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        DoubleAnimation gestureAnimation;
        private void SetGestureLabel(string GestureName)
        {
            lblGesture.Content = GestureName;
            if (gestureAnimation != null)
                BeginAnimation(System.Windows.Controls.Label.OpacityProperty, null);
            lblGesture.Content = GestureName;
            gestureAnimation = new DoubleAnimation() { From = 1, To = 0, Duration = TimeSpan.FromSeconds(4) };
            lblGesture.BeginAnimation(System.Windows.Controls.Label.OpacityProperty, gestureAnimation);
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


        #region "Audio Recognition"

        Thread audioThread;
        Stream audioStream;
        bool audioReading = false;
        SpeechRecognitionEngine speechEngine = new SpeechRecognitionEngine();
        SpeechSynthesizer speaker = new SpeechSynthesizer();
        List<string> fileNames = new List<string>();
        bool ListenForCommand = false;

        void speechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.5 && !e.Result.Text.Contains("Kinect"))
            {
                switch (e.Result.Text)
                {
                    case "Search":
                        ListenForCommand = true;
                        w8.OpenAppsMenu();
                        break;

                }
                if (ListenForCommand)
                {
                    speaker.Speak("Kinect " + e.Result.Text.ToString());

                    foreach (string str in fileNames)
                    {
                        if (str.Contains(e.Result.Text))
                        {
                            ListenForCommand = false;
                            SendKeys.SendWait(str);
                        }
                    }
                }
            }
        }

        void speechEngine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {

        }

        private void loadDictionary()
        {
            Choices choices = new Choices();

            choices.Add("Search");
            choices.Add("Kinect Search");

            var filenames = from fullFilename in Directory.EnumerateDirectories("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs")
                            select System.IO.Path.GetFileName(fullFilename);

            foreach (string filename in filenames)
            {
                choices.Add(filename);
                fileNames.Add(filename);
            }

            GrammarBuilder gb = new GrammarBuilder(choices);
            Grammar grammar = new Grammar(gb);
            speechEngine.LoadGrammar(grammar);
        }

        #endregion

    }
}