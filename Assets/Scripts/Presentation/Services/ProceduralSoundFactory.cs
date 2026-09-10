using System;
using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    public sealed class ProceduralSoundFactory
    {
        private const int SampleRate = 22050;

        private const int ClipCount = 10;

        private const float TwoPi = 2f * Mathf.PI;

        private const float VictoryToneDurationSeconds = 0.2f;

        private const float VictoryFadeSeconds = 0.02f;

        private const float DefeatToneDurationSeconds = 0.25f;

        private const float DefeatFadeSeconds = 0.02f;

        private const float GunHitSweepStartHz = 700f;

        private const float GunHitSweepRateHzPerSecond = -4375f;

        private const float StageBreakSweepStartHz = 300f;

        private const float StageBreakSweepRateHzPerSecond = -954.5454f;

        private readonly float[] _victoryFrequencies = { 523f, 659f, 784f };

        private readonly System.Random _random = new System.Random();

        public AudioClip[] CreateClips()
        {
            AudioClip[] clips = new AudioClip[ClipCount];

            clips[(int)SfxType.PistolShot] = Build("PistolShot", 0.14f, GeneratePistolShotSample);
            clips[(int)SfxType.BurstShot] = Build("BurstShot", 0.09f, GenerateBurstShotSample);
            clips[(int)SfxType.ShotgunBlast] = Build("ShotgunBlast", 0.28f, GenerateShotgunBlastSample);
            clips[(int)SfxType.GunHit] = Build("GunHit", 0.08f, GenerateGunHitSample);
            clips[(int)SfxType.Explosion] = Build("Explosion", 0.45f, GenerateExplosionSample);
            clips[(int)SfxType.StageBreak] = Build("StageBreak", 0.22f, GenerateStageBreakSample);
            clips[(int)SfxType.Reload] = Build("Reload", 0.12f, GenerateReloadSample);
            clips[(int)SfxType.Victory] = Build("Victory", 0.6f, GenerateVictorySample);
            clips[(int)SfxType.Defeat] = Build("Defeat", 0.5f, GenerateDefeatSample);
            clips[(int)SfxType.UiClick] = Build("UiClick", 0.04f, GenerateUiClickSample);

            return clips;
        }

        private AudioClip Build(string clipName, float durationSeconds, Func<float, float> sampleGenerator)
        {
            int sampleCount = Mathf.CeilToInt(durationSeconds * SampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float sampleTimeSeconds = i / (float)SampleRate;
                samples[i] = Mathf.Clamp(sampleGenerator(sampleTimeSeconds), -1f, 1f);
            }

            AudioClip audioClip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }

        private float GeneratePistolShotSample(float sampleTimeSeconds)
        {
            float noise = NextNoise() * Mathf.Exp(-sampleTimeSeconds * 35f);
            float tone = 0.6f * Mathf.Sin(TwoPi * 170f * sampleTimeSeconds) * Mathf.Exp(-sampleTimeSeconds * 30f);

            return noise + tone;
        }

        private float GenerateBurstShotSample(float sampleTimeSeconds)
        {
            float noise = NextNoise() * Mathf.Exp(-sampleTimeSeconds * 55f);
            float tone = 0.5f * Mathf.Sin(TwoPi * 230f * sampleTimeSeconds) * Mathf.Exp(-sampleTimeSeconds * 45f);

            return noise + tone;
        }

        private float GenerateShotgunBlastSample(float sampleTimeSeconds)
        {
            float noise = NextNoise() * Mathf.Exp(-sampleTimeSeconds * 14f);
            float tone = 0.8f * Mathf.Sin(TwoPi * 90f * sampleTimeSeconds) * Mathf.Exp(-sampleTimeSeconds * 12f);

            return noise + tone;
        }

        private float GenerateGunHitSample(float sampleTimeSeconds)
        {
            float sweepPhaseSeconds = GunHitSweepStartHz * sampleTimeSeconds + GunHitSweepRateHzPerSecond * 0.5f * sampleTimeSeconds * sampleTimeSeconds;
            float tone = Mathf.Sin(TwoPi * sweepPhaseSeconds) * Mathf.Exp(-sampleTimeSeconds * 30f);

            return tone;
        }

        private float GenerateExplosionSample(float sampleTimeSeconds)
        {
            float noise = NextNoise() * Mathf.Exp(-sampleTimeSeconds * 7f);
            float tone = 0.9f * Mathf.Sin(TwoPi * 70f * sampleTimeSeconds) * Mathf.Exp(-sampleTimeSeconds * 8f);

            return noise + tone;
        }

        private float GenerateStageBreakSample(float sampleTimeSeconds)
        {
            float sweepPhaseSeconds = StageBreakSweepStartHz * sampleTimeSeconds + StageBreakSweepRateHzPerSecond * 0.5f * sampleTimeSeconds * sampleTimeSeconds;
            float noise = NextNoise() * Mathf.Exp(-sampleTimeSeconds * 18f);
            float tone = Mathf.Sin(TwoPi * sweepPhaseSeconds) * Mathf.Exp(-sampleTimeSeconds * 14f);

            return noise + tone;
        }

        private float GenerateReloadSample(float sampleTimeSeconds)
        {
            bool isFirstClickWindow = sampleTimeSeconds < 0.03f;
            bool isSecondClickWindow = sampleTimeSeconds > 0.06f && sampleTimeSeconds < 0.09f;

            if (isFirstClickWindow == false && isSecondClickWindow == false)
            {
                return 0f;
            }

            return NextNoise() * 0.8f;
        }

        private float GenerateVictorySample(float sampleTimeSeconds)
        {
            int toneIndex = (int)(sampleTimeSeconds / VictoryToneDurationSeconds);

            if (toneIndex < 0)
            {
                toneIndex = 0;
            }

            if (toneIndex > _victoryFrequencies.Length - 1)
            {
                toneIndex = _victoryFrequencies.Length - 1;
            }

            float toneTimeSeconds = sampleTimeSeconds - toneIndex * VictoryToneDurationSeconds;
            float tone = Mathf.Sin(TwoPi * _victoryFrequencies[toneIndex] * toneTimeSeconds);
            float fade = ComputeWindowFade(toneTimeSeconds, VictoryToneDurationSeconds, VictoryFadeSeconds);

            return tone * fade;
        }

        private float GenerateDefeatSample(float sampleTimeSeconds)
        {
            if (sampleTimeSeconds < DefeatToneDurationSeconds)
            {
                return ComputeDefeatTone(392f, sampleTimeSeconds);
            }

            float secondToneTimeSeconds = sampleTimeSeconds - DefeatToneDurationSeconds;

            return ComputeDefeatTone(262f, secondToneTimeSeconds);
        }

        private float ComputeDefeatTone(float toneFrequency, float toneTimeSeconds)
        {
            float tone = Mathf.Sin(TwoPi * toneFrequency * toneTimeSeconds);
            float fade = ComputeWindowFade(toneTimeSeconds, DefeatToneDurationSeconds, DefeatFadeSeconds);

            return tone * fade;
        }

        private float GenerateUiClickSample(float sampleTimeSeconds)
        {
            return Mathf.Sin(TwoPi * 900f * sampleTimeSeconds) * Mathf.Exp(-sampleTimeSeconds * 60f);
        }

        private float ComputeWindowFade(float toneTimeSeconds, float toneDurationSeconds, float fadeSeconds)
        {
            float fadeIn = Mathf.Clamp01(toneTimeSeconds / fadeSeconds);
            float fadeOut = Mathf.Clamp01((toneDurationSeconds - toneTimeSeconds) / fadeSeconds);

            return Mathf.Min(fadeIn, fadeOut);
        }

        private float NextNoise()
        {
            return (float)(_random.NextDouble() * 2.0 - 1.0);
        }
    }
}
