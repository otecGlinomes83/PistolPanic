using Cysharp.Threading.Tasks;

namespace PistolPanic.Core
{
    public interface ISceneLoader
    {
        UniTask Load(string sceneName);
    }
}
