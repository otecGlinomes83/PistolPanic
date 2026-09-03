using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class ArenaState
    {
        public ArenaState(MapConfig mapConfig)
        {
            Vector2 halfSize = mapConfig.ArenaSize * 0.5f;
            Min = -halfSize;
            Max = halfSize;

            Obstacles = new ObstacleData[mapConfig.Obstacles.Count];

            for (int i = 0; i < mapConfig.Obstacles.Count; i++)
            {
                Obstacles[i] = mapConfig.Obstacles[i];
            }
        }

        public Vector2 Min { get; }

        public Vector2 Max { get; }

        public ObstacleData[] Obstacles { get; }
    }
}
