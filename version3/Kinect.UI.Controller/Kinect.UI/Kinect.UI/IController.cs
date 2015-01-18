using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.UI
{
    public interface IController
    {
        void OpenStartMenu();
        void OpenCharmBar();
        void OpenAppsMenu();
        void OpenDesktop();
        void OpenFileSearch();
        void MinimizeAllWindows();
        void UnminimizeWindows();
        void SplitScreenRight();
        void SplitScreenLeft();
        void ZoomIn();
        void ZoomOut();
        void ChangeTabForward();
        void ChangeTabBackward();
        void LockScreen();
        void VolumeUp();
        void VolumeDown();
        void VolumeMute();
        void ViewAllCurrentlyRunning();
        void DoMouseLeftClick();
        void DoMouseLeftClickDown();
        void DoMouseLeftClickUp();
        void DoMouseRightClick();
        void DoMouseScroll(bool up);
    }
}
