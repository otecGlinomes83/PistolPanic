using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class CollisionMath
    {
        private const float MinSegmentLengthSquared = 0.00001f;

        private const float MinSeparation = 0.00001f;

        private const float MinTargetDistanceSquared = 0.0001f;

        public bool SegmentIntersectsCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float radius)
        {
            Vector2 segmentVector = segmentEnd - segmentStart;
            float segmentLengthSquared = segmentVector.sqrMagnitude;

            if (segmentLengthSquared < MinSegmentLengthSquared)
            {
                return (segmentStart - circleCenter).sqrMagnitude <= radius * radius;
            }

            float clampedT = Vector2.Dot(circleCenter - segmentStart, segmentVector) / segmentLengthSquared;

            if (clampedT < 0f)
            {
                clampedT = 0f;
            }
            else if (clampedT > 1f)
            {
                clampedT = 1f;
            }

            Vector2 closestPoint = segmentStart + segmentVector * clampedT;
            Vector2 offset = closestPoint - circleCenter;

            return offset.sqrMagnitude <= radius * radius;
        }

        public bool TryGetCircleOverlap(Vector2 firstCenter, Vector2 secondCenter, float combinedRadius, out Vector2 normal, out float overlap)
        {
            Vector2 offset = secondCenter - firstCenter;

            if (offset.sqrMagnitude >= combinedRadius * combinedRadius)
            {
                normal = Vector2.zero;
                overlap = 0f;

                return false;
            }

            float offsetMagnitude = offset.magnitude;

            if (offsetMagnitude < MinSeparation)
            {
                normal = Vector2.up;
                overlap = combinedRadius;

                return true;
            }

            normal = offset / offsetMagnitude;
            overlap = combinedRadius - offsetMagnitude;

            return true;
        }

        public bool IsInsideCone(Vector2 observerPosition, float observerRotationDegrees, Vector2 targetPosition, float coneDegrees)
        {
            Vector2 toTarget = targetPosition - observerPosition;

            if (toTarget.sqrMagnitude < MinTargetDistanceSquared)
            {
                return true;
            }

            float targetAngleDegrees = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(observerRotationDegrees, targetAngleDegrees);

            return Mathf.Abs(deltaAngle) <= coneDegrees * 0.5f;
        }
    }
}
