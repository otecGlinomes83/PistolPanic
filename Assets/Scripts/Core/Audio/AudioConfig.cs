using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField]
        private float _masterVolume = 0.9f;

        [SerializeField]
        private float _shotVolume = 0.8f;

        [SerializeField]
        private float _impactVolume = 0.7f;

        [SerializeField]
        private float _uiVolume = 0.6f;

        public float MasterVolume => _masterVolume;

        public float ShotVolume => _shotVolume;

        public float ImpactVolume => _impactVolume;

        public float UiVolume => _uiVolume;
    }
}
