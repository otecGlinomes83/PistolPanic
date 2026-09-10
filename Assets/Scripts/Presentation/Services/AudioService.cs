using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class AudioService : MonoBehaviour
    {
        private const int SourceCount = 3;

        [Inject]
        private readonly AudioConfig _audioConfig = null;

        private readonly AudioSource[] _sources = new AudioSource[SourceCount];

        private AudioClip[] _clips;

        private int _nextSource;

        private void Start()
        {
            CreateSources();

            _clips = new ProceduralSoundFactory().CreateClips();
        }

        public void PlaySfx(SfxType type)
        {
            if (_clips == null)
            {
                return;
            }

            AudioSource audioSource = _sources[_nextSource];
            _nextSource = (_nextSource + 1) % _sources.Length;

            audioSource.PlayOneShot(_clips[(int)type], VolumeFor(type));
        }

        private void CreateSources()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                _sources[i] = audioSource;
            }
        }

        private float VolumeFor(SfxType type)
        {
            float groupVolume;

            if (type == SfxType.PistolShot || type == SfxType.BurstShot || type == SfxType.ShotgunBlast)
            {
                groupVolume = _audioConfig.ShotVolume;
            }
            else if (type == SfxType.GunHit || type == SfxType.Explosion || type == SfxType.StageBreak || type == SfxType.Reload)
            {
                groupVolume = _audioConfig.ImpactVolume;
            }
            else if (type == SfxType.Victory || type == SfxType.Defeat)
            {
                groupVolume = _audioConfig.MasterVolume;
            }
            else
            {
                groupVolume = _audioConfig.UiVolume;
            }

            return groupVolume * _audioConfig.MasterVolume;
        }
    }
}
