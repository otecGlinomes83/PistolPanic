using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/MapConfig")]
    public sealed class MapConfig : ScriptableObject
    {
        [SerializeField]
        private Vector2 _arenaSize = new Vector2(5.625f, 10f);

        [SerializeField]
        private Vector2 _playerSpawnPoint = new Vector2(0f, -4.2f);

        [SerializeField]
        private Vector2 _enemySpawnPoint = new Vector2(0f, 4.2f);

        [SerializeField]
        private float _playerSpawnRotationDegrees = 90f;

        [SerializeField]
        private float _spawnImpulseMin = 1.5f;

        [SerializeField]
        private float _spawnImpulseMax = 3f;

        [SerializeField]
        private float _spawnAngularVelocityMin = -120f;

        [SerializeField]
        private float _spawnAngularVelocityMax = 120f;

        [SerializeField]
        private float _gunCollisionRadius = 0.4f;

        [SerializeField]
        private float _gracePeriodSeconds = 1f;

        [SerializeField]
        private List<ObstacleData> _obstacles = new List<ObstacleData>();

        public Vector2 ArenaSize => _arenaSize;

        public Vector2 PlayerSpawnPoint => _playerSpawnPoint;

        public Vector2 EnemySpawnPoint => _enemySpawnPoint;

        public float PlayerSpawnRotationDegrees => _playerSpawnRotationDegrees;

        public float SpawnImpulseMin => _spawnImpulseMin;

        public float SpawnImpulseMax => _spawnImpulseMax;

        public float SpawnAngularVelocityMin => _spawnAngularVelocityMin;

        public float SpawnAngularVelocityMax => _spawnAngularVelocityMax;

        public float GunCollisionRadius => _gunCollisionRadius;

        public float GracePeriodSeconds => _gracePeriodSeconds;

        public IReadOnlyList<ObstacleData> Obstacles => _obstacles;
    }
}
