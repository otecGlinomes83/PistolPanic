using System;

namespace PistolPanic.Core
{
    public sealed class DuelFlow
    {
        private float _graceRemainingSeconds;

        private float _fightElapsedSeconds;

        public event Action<DuelPhase> PhaseChanged;

        public DuelPhase Phase { get; private set; } = DuelPhase.Armed;

        public float FightElapsedSeconds => _fightElapsedSeconds;

        public bool IsSimulationRunning
        {
            get
            {
                if (Phase == DuelPhase.Grace)
                {
                    return true;
                }

                if (Phase == DuelPhase.Fight)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsFireAllowed
        {
            get
            {
                return Phase == DuelPhase.Fight;
            }
        }

        public void StartGrace(float graceSeconds)
        {
            if (Phase != DuelPhase.Armed)
            {
                return;
            }

            Phase = DuelPhase.Grace;
            _graceRemainingSeconds = graceSeconds;
            PhaseChanged?.Invoke(DuelPhase.Grace);
        }

        public void Tick(float fixedDelta)
        {
            if (Phase == DuelPhase.Grace)
            {
                TickGraceCountdown(fixedDelta);

                return;
            }

            if (Phase == DuelPhase.Fight)
            {
                _fightElapsedSeconds += fixedDelta;
            }
        }

        private void TickGraceCountdown(float fixedDelta)
        {
            _graceRemainingSeconds -= fixedDelta;

            if (_graceRemainingSeconds > 0f)
            {
                return;
            }

            Phase = DuelPhase.Fight;
            PhaseChanged?.Invoke(DuelPhase.Fight);
        }

        public void CompleteVictory()
        {
            if (Phase != DuelPhase.Fight)
            {
                return;
            }

            Phase = DuelPhase.Victory;
            PhaseChanged?.Invoke(DuelPhase.Victory);
        }

        public void CompleteDefeat()
        {
            if (Phase != DuelPhase.Fight)
            {
                return;
            }

            Phase = DuelPhase.Defeat;
            PhaseChanged?.Invoke(DuelPhase.Defeat);
        }
    }
}
