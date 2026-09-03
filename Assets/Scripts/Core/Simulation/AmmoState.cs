namespace PistolPanic.Core
{
    public sealed class AmmoState
    {
        public int Magazine { get; set; }

        public float RegenTimerSeconds { get; set; }

        public float ReloadRemainingSeconds { get; set; }

        public bool IsFullReloading { get; set; }

        public float CooldownRemainingSeconds { get; set; }
    }
}
