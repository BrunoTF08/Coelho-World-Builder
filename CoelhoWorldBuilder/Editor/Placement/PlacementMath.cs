using System;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public static class PlacementMath
    {
        public static Vector2 Disk(System.Random random, float radius)
        {
            double angle = random.NextDouble() * Math.PI * 2;
            double distance = Math.Sqrt(random.NextDouble()) * radius;
            return new Vector2((float)(Math.Cos(angle) * distance), (float)(Math.Sin(angle) * distance));
        }
        public static float Range(System.Random random, Vector2 limits)
        { return Mathf.Lerp(limits.x, limits.y, (float)random.NextDouble()); }
    }
}
