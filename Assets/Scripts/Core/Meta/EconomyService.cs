namespace PistolPanic.Meta
{
    public sealed class EconomyService
    {
        private readonly EconomyConfig _economyConfig;

        public EconomyService(EconomyConfig economyConfig)
        {
            _economyConfig = economyConfig;
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
