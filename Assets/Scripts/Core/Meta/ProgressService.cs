using System;

namespace PistolPanic.Meta
{
    public sealed class ProgressService
    {
        private int _currentLevel = 1;

        public event Action<int> LevelChanged;

        public int CurrentLevel => _currentLevel;

        public void AdvanceLevel()
        {
            _currentLevel += 1;
            LevelChanged?.Invoke(_currentLevel);
        }
    }
}
