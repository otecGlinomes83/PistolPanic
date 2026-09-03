using System;
using UnityEngine;

namespace PistolPanic.Core
{
    [Serializable]
    public sealed class ObstacleData
    {
        public Vector2 Center;

        public float Radius = 0.5f;
    }
}
