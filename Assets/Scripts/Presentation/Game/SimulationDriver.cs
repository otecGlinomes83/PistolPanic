using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class SimulationDriver : MonoBehaviour
    {
        [Inject]
        private readonly SimulationConfig _simulationConfig = null;

        [Inject]
        private readonly DuelSim _duelSim = null;

        private float _fixedDeltaSeconds;

        private float _maxAccumulatorSeconds;

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
            _fixedDeltaSeconds = 1f / _simulationConfig.TicksPerSecond;
            _maxAccumulatorSeconds = _fixedDeltaSeconds * _simulationConfig.MaxStepsPerFrame;

            Debug.Log("SimulationDriver: start, fixedStep=" + _fixedDeltaSeconds.ToString("0.####") + " maxSteps=" + _simulationConfig.MaxStepsPerFrame);
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

            while (_accumulatorSeconds >= _fixedDeltaSeconds && stepsThisFrame < _simulationConfig.MaxStepsPerFrame)
            {
                _duelSim.Tick(_fixedDeltaSeconds);
                _accumulatorSeconds -= _fixedDeltaSeconds;
                stepsThisFrame += 1;
                _tickCounter += 1;
            }

            if (_accumulatorSeconds > _maxAccumulatorSeconds)
            {
                _accumulatorSeconds = _maxAccumulatorSeconds;
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
