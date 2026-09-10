using PistolPanic.Core;

namespace PistolPanic.Meta
{
    public readonly struct MatchResult
    {
        public MatchResult(DuelPhase phase, int reward)
        {
            Phase = phase;
            Reward = reward;
        }

        public DuelPhase Phase { get; }

        public int Reward { get; }
    }
}
