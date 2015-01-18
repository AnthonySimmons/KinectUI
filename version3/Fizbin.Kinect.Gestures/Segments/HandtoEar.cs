using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class HandToEar : IRelativeGestureSegment
    {
        private const float AccuracyBuffer = (float)0.125;

        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            //hand around the height of the head
            if (skeleton.Joints[JointType.HandLeft].Position.Z <= (skeleton.Joints[JointType.Head].Position.Z + AccuracyBuffer) &&
                skeleton.Joints[JointType.HandLeft].Position.Z >= (skeleton.Joints[JointType.Head].Position.Z - AccuracyBuffer))
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y <= (skeleton.Joints[JointType.Head].Position.Y + AccuracyBuffer) &&
                    skeleton.Joints[JointType.HandLeft].Position.Y >= (skeleton.Joints[JointType.Head].Position.Y - (2*AccuracyBuffer)))
                {
                    if (Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X) <= (Math.Abs(skeleton.Joints[JointType.Head].Position.X) + (3 * AccuracyBuffer)) &&
                        Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X) >= (Math.Abs(skeleton.Joints[JointType.Head].Position.X)))
                    {
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }
            }
            //hand left of shoulder
            //hand at ear
            //not all logic evaluated to true
            return GesturePartResult.Fail;
        }
    }
}
