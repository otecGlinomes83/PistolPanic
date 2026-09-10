using System;

namespace PistolPanic.Meta
{
    public sealed class EconomyService
    {
        private readonly EconomyConfig _economyConfig;

        private int _money;

        public EconomyService(EconomyConfig economyConfig)
        {
            _economyConfig = economyConfig;
        }

        public event Action<int> MoneyChanged;

        public int Money => _money;

        public void AddMoney(int amount)
        {
            _money += amount;

            MoneyChanged?.Invoke(_money);
        }

        public int GetVictoryReward(int completedLevel)
        {
            int levelIndex = completedLevel - 1;

            if (levelIndex < 0)
            {
                levelIndex = 0;
            }

            return _economyConfig.BaseReward + _economyConfig.RewardPerLevel * levelIndex;
        }
    }
}
