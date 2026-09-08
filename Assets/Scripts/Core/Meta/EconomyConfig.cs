using UnityEngine;

namespace PistolPanic.Meta
{
    [CreateAssetMenu(menuName = "PistolPanic/EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [SerializeField]
        private int _baseReward = 25;

        [SerializeField]
        private int _rewardPerLevel = 2;

        public int BaseReward => _baseReward;

        public int RewardPerLevel => _rewardPerLevel;
    }
}
