using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct ShotFiredSnapshot
    {
        public ShotFiredSnapshot(Vector2 position, bool isPlayerOwned, FirePattern pattern)
        {
            Position = position;
            IsPlayerOwned = isPlayerOwned;
            Pattern = pattern;
        }

        public Vector2 Position { get; }

        public bool IsPlayerOwned { get; }

        public FirePattern Pattern { get; }
    }
}
