using PistolPanic.Core;

namespace PistolPanic.Meta
{
    public sealed class EnemyTypeProvider
    {
        private readonly EnemyTypeConfig _current;

        public EnemyTypeProvider(EnemyCatalog enemyCatalog, ProgressService progressService)
        {
            int levelIndex = progressService.CurrentLevel - 1;
            int typeIndex = levelIndex % enemyCatalog.Types.Count;

            _current = enemyCatalog.Types[typeIndex];
        }

        public EnemyTypeConfig Current => _current;
    }
}
