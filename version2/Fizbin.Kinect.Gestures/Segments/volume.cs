using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class volumeUp : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // hand above shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ShoulderLeft].Position.Y)
            {
                return GesturePartResult.Succeed;
            }
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    public class volumeDown : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // hand above shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipLeft].Position.Y)
            {
                return GesturePartResult.Succeed;
            }
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }
}
