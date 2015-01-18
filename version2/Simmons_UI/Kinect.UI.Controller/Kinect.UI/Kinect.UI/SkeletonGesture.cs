using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Fizbin.Kinect.Gestures.Segments;
using System.Timers;
using System.ComponentModel;

namespace Kinect.UI
{
    class SkeletonGesture
    {

    }

    public int trackedID;
        public bool hit = false;     
        Timer _clearTimer;
        public String Gesture = "";

        // skeleton gesture recognizer
        public GestureController gestureController;

        public bool focusGranted = false;

        public SkeletonGestures(Skeleton data)
        {
            trackedID = data.TrackingId;
            initGestures();
            _clearTimer = new Timer(10000);
            _clearTimer.Elapsed += new ElapsedEventHandler(clearTimer_Elapsed);
        }

        private void initGestures()
        {
            // initialize the gesture recognizer
            gestureController = new GestureController();
            gestureController.GestureRecognized += OnGestureRecognized;

            // register the gestures for this demo
            RegisterGestures();
        }

         /// <summary>
        /// Helper function to register all available 
        /// </summary>
        private void RegisterGestures() //here
        {
            // define the gestures for the demo
            // Focus Gesture
            IRelativeGestureSegment[] focusSegments = new IRelativeGestureSegment[4];
            focusSegments[0] = new FocusGesture();
            focusSegments[1] = new FocusGesture2();
            focusSegments[2] = new FocusGesture3();
            focusSegments[3] = new FocusGesture4();
            gestureController.AddGesture("FocusGesture", focusSegments);

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

            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            waveLeftSegments[4] = waveLeftSegment1;
            waveLeftSegments[5] = waveLeftSegment2;
            gestureController.AddGesture("WaveLeft", waveLeftSegments);
        }

        #region Event Handlers

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Gesture event arguments.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            switch (e.GestureName)
            {
                case "FocusGesture":
                    Gesture = "Focus Granted";
                    this.focusGranted = true;
                    _clearTimer.Start();
                    break;
                /*case "Menu":
                    base.Gesture = "Menu";
                    break;*/
                case "WaveRight":
                    Gesture = "Wave Right";
                    break;
                case "WaveLeft":
                    Gesture = "Wave Left";
                    break;
                default:
                    break;
            }
            System.Media.SystemSounds.Beep.Play();
        }

        /// <summary>
        /// Clear text after some time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clearTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Gesture = "";
            _clearTimer.Stop();
            this.focusGranted = false;
        }

         #endregion Event Handlers
}