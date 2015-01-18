using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class SwipeLeftSegment1 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // //left hand in front of left Shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z && 
                skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z)
            {
                // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - PASS");
                // //left hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y && 
                    skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - PASS");
                    // //left hand left of left Shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }

                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }

    /// <summary>
    /// The second part of the swipe right gesture
    /// </summary>
    public class SwipeLeftSegment2 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // //left hand in front of left Shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z &&
                skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z)
            {
                // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - PASS");
                // //left hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y &&
                    skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - PASS");
                    // //left hand left of left Shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X &&
                        skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }

                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }

    /// <summary>
    /// The third part of the swipe right gesture
    /// </summary>
    public class SwipeLeftSegment3 : IRelativeGestureSegment
    {
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // //left hand in front of left Shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z &&
                skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z)
            {
                // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - PASS");
                // //left hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y &&
                    skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - PASS");
                    // //left hand left of left Shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X &&
                        skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }

                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }
}
