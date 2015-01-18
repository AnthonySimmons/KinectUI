using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class ScrollUp1 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ShoulderLeft].Position.X)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.Spine].Position.Y)
                {
                    return GesturePartResult.Succeed;
                }
                return GesturePartResult.Pausing;
            }
            // hand has not dropped but is not quite where we expect it to be, pausing till next frame
            return GesturePartResult.Fail;
            //}
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    public class ScrollUp2 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y &&
                    skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Spine].Position.Y)
                {
                    return GesturePartResult.Succeed;
                }
                return GesturePartResult.Pausing;
            }
            // hand has not dropped but is not quite where we expect it to be, pausing till next frame
            return GesturePartResult.Fail;
            //}
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    public class ScrollUp3 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // hand above shoulder
            //if (skeleton.Joints[JointType.HandLeft].Position.Z > skeleton.Joints[JointType.Spine].Position.Z)
            //{
            // hand left of shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.Head].Position.Y &&
                    skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ShoulderLeft].Position.Y)
                {
                    return GesturePartResult.Succeed;
                }
                return GesturePartResult.Pausing;
            }
            // hand has not dropped but is not quite where we expect it to be, pausing till next frame
            return GesturePartResult.Fail;
            //}
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    public class ScrollUp4 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
                {
                    return GesturePartResult.Succeed;
                }
                return GesturePartResult.Pausing;
            }
            // hand has not dropped but is not quite where we expect it to be, pausing till next frame
            return GesturePartResult.Fail;
        }
    }

}
