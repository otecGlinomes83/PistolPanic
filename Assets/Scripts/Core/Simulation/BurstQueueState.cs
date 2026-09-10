namespace PistolPanic.Core
{
    public sealed class BurstQueueState
    {
        public bool IsActive { get; set; }

        public int ShotsRemaining { get; set; }

        public float TimerSeconds { get; set; }
    }
}
