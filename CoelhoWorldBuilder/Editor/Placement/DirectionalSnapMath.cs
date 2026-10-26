using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public enum SnapDirection { Right, Left, Forward, Back }
    public static class DirectionalSnapMath
    {
        public static Vector3 Connect(Vector3 referencePoint, Vector3 nextPoint, Quaternion rotation, Vector3 scale, Vector3 offset)
            => referencePoint - rotation * Vector3.Scale(nextPoint, scale) + offset;
        public static SnapDirection OpposingPoint(SnapDirection direction, Quaternion referenceRotation, Quaternion nextRotation)
        {
            var axis = direction == SnapDirection.Right ? Vector3.right : direction == SnapDirection.Left ? Vector3.left
                : direction == SnapDirection.Forward ? Vector3.forward : Vector3.back;
            var local = Quaternion.Inverse(nextRotation) * -(referenceRotation * axis);
            return Mathf.Abs(local.x) >= Mathf.Abs(local.z) ? (local.x >= 0 ? SnapDirection.Right : SnapDirection.Left)
                : (local.z >= 0 ? SnapDirection.Forward : SnapDirection.Back);
        }
        public static SnapDirection Direction(Vector3 pointer, Bounds reference, Quaternion orientation)
        {
            var delta = Quaternion.Inverse(Quaternion.Euler(0, orientation.eulerAngles.y, 0)) * (pointer - reference.center);
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.z)) return delta.x >= 0 ? SnapDirection.Right : SnapDirection.Left;
            return delta.z >= 0 ? SnapDirection.Forward : SnapDirection.Back;
        }
        public static Vector3 Adjacent(Bounds referenceLocal, Matrix4x4 referenceMatrix, Bounds nextLocal,
            Quaternion nextRotation, Vector3 nextScale, SnapDirection direction, bool edgeAlignment, Vector3 offset)
        {
            // Project both boxes into the reference's horizontal axes. Only translate the next piece.
            var forward = referenceMatrix.MultiplyVector(Vector3.forward);
            float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            var axes = Quaternion.Euler(0, yaw, 0);
            var toAxes = Matrix4x4.Rotate(Quaternion.Inverse(axes));
            var a = SmartPlacementMath.TransformBounds(referenceLocal, toAxes * referenceMatrix);
            var b = SmartPlacementMath.TransformBounds(nextLocal, toAxes * Matrix4x4.TRS(Vector3.zero, nextRotation, nextScale));
            var position = a.center - b.center;
            position.y = a.min.y - b.min.y;
            switch (direction)
            {
                case SnapDirection.Right: position.x = a.max.x - b.min.x; break;
                case SnapDirection.Left: position.x = a.min.x - b.max.x; break;
                case SnapDirection.Forward: position.z = a.max.z - b.min.z; break;
                case SnapDirection.Back: position.z = a.min.z - b.max.z; break;
            }
            // Edge alignment aligns the perpendicular minimum edges; disabled means center alignment.
            if (edgeAlignment)
            {
                if (direction == SnapDirection.Right || direction == SnapDirection.Left) position.z = a.min.z - b.min.z;
                else position.x = a.min.x - b.min.x;
            }
            return axes * position + offset;
        }
    }
}
