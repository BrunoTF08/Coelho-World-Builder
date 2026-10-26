using System;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public enum SnapMode { Grid, Free, Surface }
    public enum AnchorMode { Pivot, BottomCenter, BottomCorner, Custom }
    [Serializable]
    public sealed class PrefabPlacementProfile
    {
        public string guid;
        public Vector2 cell = Vector2.one;
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public bool alignToSurface;
        public AnchorMode anchorMode = AnchorMode.BottomCorner;
        public Vector3 customAnchor;
        public bool snap = true;
        public bool manualDimensions;
        public ModularCategory category;
        public bool useConnectionPoints;
        public Vector3 rightConnection, leftConnection, forwardConnection, backConnection;
        public Vector3[] ConnectionPoints() => new[] { rightConnection, leftConnection, forwardConnection, backConnection };
    }
}
