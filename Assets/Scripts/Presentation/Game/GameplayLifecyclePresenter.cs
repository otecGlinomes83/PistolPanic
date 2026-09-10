using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class GameplayLifecyclePresenter : MonoBehaviour
    {
        [Inject]
        private readonly DuelFlow _duelFlow = null;

        [Inject]
        private readonly IPlatformLifecycle _platformLifecycle = null;

        private void Start()
        {
            _duelFlow.PhaseChanged += OnDuelPhaseChanged;
        }

        private void OnDestroy()
        {
            _duelFlow.PhaseChanged -= OnDuelPhaseChanged;
        }

        private void OnDuelPhaseChanged(DuelPhase phase)
        {
            if (phase == DuelPhase.Armed)
            {
                _platformLifecycle.GameplayStop();
            }
            else
            {
                _platformLifecycle.GameplayStart();
            }
        }
    }
}
