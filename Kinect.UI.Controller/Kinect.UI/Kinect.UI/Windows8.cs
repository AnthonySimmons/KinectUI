using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace Kinect.UI
{
    public class Windows8 : IController
    {
        
        //Windows
        public void OpenStartMenu()
        {
            SendKey(new List<Keys> { Keys.LWin });
        }

        //Windows + C
        public void OpenCharmBar()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.C });
        }

        public void OpenAppsMenu()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.Q });
        }

        public void OpenDesktop()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.D });
        }

        public void OpenFileSearch()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.F });
        }
        
        public void MinimizeAllWindows()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.M });
        }
        public void UnminimizeWindows()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.LShiftKey, Keys.M });
        }

        public void SplitScreenRight()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.OemPeriod });
        }

        public void SplitScreenLeft()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.LShiftKey, Keys.OemPeriod });
        }

        public void ZoomIn()
        {
            SendKey(new List<Keys> { Keys.LControlKey, Keys.Add });
        }

        public void ZoomOut()
        {
            SendKey(new List<Keys> { Keys.LControlKey, Keys.Subtract });
        }

        public void ChangeTabForward()
        {
            SendKey(new List<Keys> { Keys.LControlKey, Keys.Tab });
        }

        public void ChangeTabBackward()
        {
            SendKey(new List<Keys> { Keys.LControlKey, Keys.LShiftKey, Keys.Tab });
        }

        public void LockScreen()
        {
            SendKey(new List<Keys> { Keys.LWin, Keys.L });
        }

        public void VolumeUp()
        {
            SendKey(new List<Keys> { Keys.VolumeUp });
        }

        public void VolumeDown()
        {
            SendKey(new List<Keys> { Keys.VolumeDown});
        }

        public void VolumeMute()
        {
            SendKey(new List<Keys> { Keys.VolumeMute });
        }

        public void DoMouseLeftClick()
        {
            uint X = (uint)System.Windows.Forms.Cursor.Position.X;
            uint Y = (uint)System.Windows.Forms.Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        public void DoMouseLeftClickDown()
        {
            uint X = (uint)System.Windows.Forms.Cursor.Position.X;
            uint Y = (uint)System.Windows.Forms.Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
        }

        public void DoMouseLeftClickUp()
        {
            uint X = (uint)System.Windows.Forms.Cursor.Position.X;
            uint Y = (uint)System.Windows.Forms.Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        public void DoMouseRightClick()
        {
            uint X = (uint)System.Windows.Forms.Cursor.Position.X;
            uint Y = (uint)System.Windows.Forms.Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
        }

        public void DoMouseScroll(bool up)
        {
            if (up) mouse_event(MOUSEEVENTF_SCROLL, 0, 0, 120, 0);
            else mouse_event(MOUSEEVENTF_SCROLL, 0, 0, unchecked((uint)-120), 0);
        }

        public void ViewAllCurrentlyRunning()
        {
            //KeyDown(Keys.LWin);
            //KeyDown(Keys.LControlKey);
            KeyDown(Keys.LMenu | Keys.Tab);
            //keybd_event((byte)(Keys.LMenu | Keys.Tab), 0, 0, 0);
            //keybd_event((byte)Keys.Tab, 1, 0, 0);
            //KeyDown(Keys.Tab);
            KeyUp(Keys.Tab);
            
            Thread.Sleep(3000);
            //keybd_event((byte)Keys.LMenu, 0, KEYEVENTF_KEYUP, 0);
            KeyUp(Keys.LMenu  | Keys.Tab);
            //KeyUp(Keys.LControlKey);
            //KeyUp(Keys.LWin);
        }

        private void SendKey(List<Keys> keys)
        {
            foreach (Keys k in keys)
                KeyDown(k);
            foreach (Keys k in keys)
                KeyUp(k);
            ////SendKeys.SendWait(key);
            //KeyDown(Keys.LWin);
            ////KeyDown(Keys.D4);
            //KeyUp(Keys.LWin);
            ////KeyUp(Keys.D4);
        }

        #region "Key Events"
        [DllImport("user32.dll", CharSet=CharSet.Auto, CallingConvention=CallingConvention.StdCall)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        private static void KeyDown(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        private static void KeyUp(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
        #endregion

        #region "Mouse Events"
        [DllImport("user32")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_SCROLL = 0x800;
        #endregion

    }
}
