#if UNITY_EDITOR
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class SimulationDebugOverlay : MonoBehaviour
    {
        [Inject]
        private readonly SimulationDriver _simulationDriver = null;

        private void OnGUI()
        {
            Rect labelRect = new Rect(16f, 16f, 420f, 40f);
            GUI.Label(labelRect, "Sim ticks: " + _simulationDriver.TicksPerSecond + " per second");
        }
    }
}
#endif
