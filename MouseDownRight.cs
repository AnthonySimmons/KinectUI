using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class MouseDownRight : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // hand below shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                // hand left of shoulder
                if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                {
                    if (skeleton.Joints[JointType.HandLeft].Position.Z > skeleton.Joints[JointType.ElbowLeft].Position.Z)
                    {
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }
                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Fail;
            }
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }


    public class MouseDownRight2 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // hand below shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                // hand left of shoulder
                if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                {
                    if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z)
                    {
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }
                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Fail;
            }
            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }



    }

}
