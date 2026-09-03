using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class SimulationDriver : MonoBehaviour
    {
        private const float FixedDeltaSeconds = 1f / 60f;

        private const int MaxStepsPerFrame = 3;

        [Inject]
        private readonly DuelSim _duelSim = null;

        private float _accumulatorSeconds;

        private float _simTimeScale = 1f;

        private int _tickCounter;

        private float _ticksPerSecondTimer;

        public int TicksPerSecond { get; private set; }

        public float TimeScale
        {
            get
            {
                return _simTimeScale;
            }

            set
            {
                _simTimeScale = value;
            }
        }

        private void Start()
        {
            Debug.Log("SimulationDriver: start, fixedStep=" + FixedDeltaSeconds.ToString("0.####") + " maxSteps=" + MaxStepsPerFrame);
        }

        private void Update()
        {
            AccumulateAndTick();
            UpdateTicksPerSecond();
        }

        private void AccumulateAndTick()
        {
            _accumulatorSeconds += Time.unscaledDeltaTime * _simTimeScale;

            int stepsThisFrame = 0;

            while (_accumulatorSeconds >= FixedDeltaSeconds && stepsThisFrame < MaxStepsPerFrame)
            {
                _duelSim.Tick(FixedDeltaSeconds);
                _accumulatorSeconds -= FixedDeltaSeconds;
                stepsThisFrame += 1;
                _tickCounter += 1;
            }

            if (_accumulatorSeconds >= FixedDeltaSeconds)
            {
                _accumulatorSeconds = 0f;
            }
        }

        private void UpdateTicksPerSecond()
        {
            _ticksPerSecondTimer += Time.unscaledDeltaTime;

            if (_ticksPerSecondTimer < 1f)
            {
                return;
            }

            TicksPerSecond = _tickCounter;
            _tickCounter = 0;
            _ticksPerSecondTimer -= 1f;
        }
    }
}
