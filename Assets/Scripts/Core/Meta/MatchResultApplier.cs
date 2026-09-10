using System;
using PistolPanic.Core;

namespace PistolPanic.Meta
{
    public sealed class MatchResultApplier : IDisposable
    {
        private readonly DuelFlow _duelFlow;

        private readonly ProgressService _progressService;

        private readonly EconomyService _economyService;

        public event Action<MatchResult> MatchFinished;

        public MatchResultApplier(DuelFlow duelFlow, ProgressService progressService, EconomyService economyService)
        {
            _duelFlow = duelFlow;
            _progressService = progressService;
            _economyService = economyService;

            _duelFlow.PhaseChanged += OnDuelPhaseChanged;
        }

        public void Dispose()
        {
            _duelFlow.PhaseChanged -= OnDuelPhaseChanged;
        }

        private void OnDuelPhaseChanged(DuelPhase phase)
        {
            if (phase == DuelPhase.Victory)
            {
                ApplyVictory();
            }
            else if (phase == DuelPhase.Defeat)
            {
                ApplyDefeat();
            }
        }

        private void ApplyVictory()
        {
            int reward = _economyService.GetVictoryReward(_progressService.CurrentLevel);

            _progressService.AdvanceLevel();
            _economyService.AddMoney(reward);

            MatchFinished?.Invoke(new MatchResult(DuelPhase.Victory, reward));
        }

        private void ApplyDefeat()
        {
            _progressService.AdvanceLevel();

            MatchFinished?.Invoke(new MatchResult(DuelPhase.Defeat, 0));
        }
    }
}
