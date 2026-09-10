using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using UnityEngine;
using YG;

namespace PistolPanic.Presentation
{
    public sealed class YgLifecycleAdapter : IPlatformLifecycle
    {
        private const int SdkWaitTimeoutSeconds = 10;

        public UniTask WaitForDataLoadedAsync()
        {
            if (YG2.isSDKEnabled)
            {
                Debug.Log("YgLifecycle: SDK already enabled");

                return UniTask.CompletedTask;
            }

            Debug.Log("YgLifecycle: polling for SDK data");

            return UniTask.WaitUntil(IsSdkEnabled).Timeout(TimeSpan.FromSeconds(SdkWaitTimeoutSeconds));
        }

        public void MarkReady()
        {
            Debug.Log("YgLifecycle: MarkReady");

            YG2.GameReadyAPI();
        }

        public void GameplayStart()
        {
            Debug.Log("YgLifecycle: GameplayStart");

            YG2.GameplayStart();
        }

        public void GameplayStop()
        {
            Debug.Log("YgLifecycle: GameplayStop");

            YG2.GameplayStop();
        }

        private bool IsSdkEnabled()
        {
            return YG2.isSDKEnabled;
        }
    }
}
