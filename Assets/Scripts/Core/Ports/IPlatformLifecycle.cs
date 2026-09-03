using Cysharp.Threading.Tasks;

namespace PistolPanic.Core
{
    public interface IPlatformLifecycle
    {
        UniTask WaitForDataLoadedAsync();

        void MarkReady();

        void GameplayStart();

        void GameplayStop();
    }
}
