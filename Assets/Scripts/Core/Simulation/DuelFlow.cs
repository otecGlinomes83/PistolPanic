using System;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class DuelFlow
    {
        private float _graceRemainingSeconds;

        public event Action<DuelPhase> PhaseChanged;

        public DuelPhase Phase { get; private set; } = DuelPhase.Armed;

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

            Debug.Log("DuelFlow: Armed -> Grace");
        }

        public void Tick(float fixedDelta)
        {
            if (Phase != DuelPhase.Grace)
            {
                return;
            }

            _graceRemainingSeconds -= fixedDelta;

            if (_graceRemainingSeconds > 0f)
            {
                return;
            }

            Phase = DuelPhase.Fight;
            PhaseChanged?.Invoke(DuelPhase.Fight);

            Debug.Log("DuelFlow: Grace -> Fight");
        }

        public void CompleteVictory()
        {
            if (Phase != DuelPhase.Fight)
            {
                return;
            }

            Phase = DuelPhase.Victory;
            PhaseChanged?.Invoke(DuelPhase.Victory);

            Debug.Log("DuelFlow: Fight -> Victory");
        }

        public void CompleteDefeat()
        {
            if (Phase != DuelPhase.Fight)
            {
                return;
            }

            Phase = DuelPhase.Defeat;
            PhaseChanged?.Invoke(DuelPhase.Defeat);

            Debug.Log("DuelFlow: Fight -> Defeat");
        }
    }
}
