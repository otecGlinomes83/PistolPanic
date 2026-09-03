namespace PistolPanic.Core
{
    public sealed class EnemyHealthState
    {
        public int StageIndex { get; set; }

        public float StageHealthRemaining { get; set; }

        public int TotalStages { get; set; }

        public bool IsDefeated
        {
            get
            {
                return StageIndex >= TotalStages - 1 && StageHealthRemaining <= 0f;
            }
        }
    }
}
