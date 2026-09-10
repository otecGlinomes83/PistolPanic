using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class MovementSystem
    {
        private const float TorqueSignThreshold = 0.5f;

        private readonly PhysicsConfig _physicsConfig;

        private readonly CollisionMath _collisionMath;

        public MovementSystem(PhysicsConfig physicsConfig, CollisionMath collisionMath)
        {
            _physicsConfig = physicsConfig;
            _collisionMath = collisionMath;
        }

        public void Tick(float fixedDelta, ArenaState arena, GunState gun)
        {
            ApplyDamping(fixedDelta, gun);
            ApplyInertia(fixedDelta, gun);
            ApplyAngularVelocity(fixedDelta, gun);
            ResolveWallCollisions(arena, gun);
            ResolveObstacleCollisions(arena, gun);
        }

        private void ApplyDamping(float fixedDelta, GunState gun)
        {
            float linearFactor = 1f - _physicsConfig.LinearDampingPerSecond * fixedDelta;

            if (linearFactor < 0f)
            {
                linearFactor = 0f;
            }

            gun.Velocity *= linearFactor;

            if (gun.Velocity.sqrMagnitude < _physicsConfig.MinLinearSpeedUnits * _physicsConfig.MinLinearSpeedUnits)
            {
                gun.Velocity = Vector2.zero;
            }

            float angularFactor = 1f - _physicsConfig.AngularDampingPerSecond * fixedDelta;

            if (angularFactor < 0f)
            {
                angularFactor = 0f;
            }

            gun.AngularVelocityDegrees *= angularFactor;

            if (Mathf.Abs(gun.AngularVelocityDegrees) < _physicsConfig.MinAngularSpeedDegrees)
            {
                gun.AngularVelocityDegrees = 0f;
            }
        }

        public void ApplyRecoil(GunState gun, Vector2 fireDirection, WeaponParams weaponParams)
        {
            gun.Velocity -= fireDirection * weaponParams.RecoilImpulse;

            float torqueMagnitude = Random.Range(weaponParams.RecoilTorqueMinDegrees, weaponParams.RecoilTorqueMaxDegrees);
            float torqueSign;

            if (Random.value < TorqueSignThreshold)
            {
                torqueSign = -1f;
            }
            else
            {
                torqueSign = 1f;
            }

            gun.AngularVelocityDegrees += torqueSign * torqueMagnitude;
        }

        private void ApplyInertia(float fixedDelta, GunState gun)
        {
            gun.Position += gun.Velocity * fixedDelta;
        }

        private void ApplyAngularVelocity(float fixedDelta, GunState gun)
        {
            gun.RotationDegrees += gun.AngularVelocityDegrees * fixedDelta;
        }

        private void ResolveWallCollisions(ArenaState arena, GunState gun)
        {
            Vector2 position = gun.Position;
            Vector2 velocity = gun.Velocity;

            if (position.x < arena.Min.x + gun.Radius)
            {
                position.x = arena.Min.x + gun.Radius;

                if (velocity.x < 0f)
                {
                    velocity.x = -velocity.x;
                }
            }
            else if (position.x > arena.Max.x - gun.Radius)
            {
                position.x = arena.Max.x - gun.Radius;

                if (velocity.x > 0f)
                {
                    velocity.x = -velocity.x;
                }
            }

            if (position.y < arena.Min.y + gun.Radius)
            {
                position.y = arena.Min.y + gun.Radius;

                if (velocity.y < 0f)
                {
                    velocity.y = -velocity.y;
                }
            }
            else if (position.y > arena.Max.y - gun.Radius)
            {
                position.y = arena.Max.y - gun.Radius;

                if (velocity.y > 0f)
                {
                    velocity.y = -velocity.y;
                }
            }

            gun.Position = position;
            gun.Velocity = velocity;
        }

        private void ResolveObstacleCollisions(ArenaState arena, GunState gun)
        {
            ObstacleData[] obstacles = arena.Obstacles;

            for (int i = 0; i < obstacles.Length; i++)
            {
                ResolveObstacleCollision(obstacles[i], gun);
            }
        }

        private void ResolveObstacleCollision(ObstacleData obstacle, GunState gun)
        {
            float combinedRadius = obstacle.Radius + gun.Radius;
            Vector2 normal;
            float overlap;

            bool isOverlapping = _collisionMath.TryGetCircleOverlap(obstacle.Center, gun.Position, combinedRadius, out normal, out overlap);

            if (isOverlapping == false)
            {
                return;
            }

            Vector2 position = obstacle.Center + normal * combinedRadius;
            float velocityAlongNormal = Vector2.Dot(gun.Velocity, normal);
            Vector2 velocity = gun.Velocity;

            if (velocityAlongNormal < 0f)
            {
                velocity -= normal * (2f * velocityAlongNormal);
            }

            gun.Position = position;
            gun.Velocity = velocity;
        }

        public void ResolveGunPairCollision(GunState firstGun, GunState secondGun)
        {
            float combinedRadius = firstGun.Radius + secondGun.Radius;
            Vector2 normal;
            float overlap;

            bool isOverlapping = _collisionMath.TryGetCircleOverlap(secondGun.Position, firstGun.Position, combinedRadius, out normal, out overlap);

            if (isOverlapping == false)
            {
                return;
            }

            float halfOverlap = overlap * 0.5f;
            firstGun.Position += normal * halfOverlap;
            secondGun.Position -= normal * halfOverlap;

            float firstVelocityAlongNormal = Vector2.Dot(firstGun.Velocity, normal);
            float secondVelocityAlongNormal = Vector2.Dot(secondGun.Velocity, normal);

            if (firstVelocityAlongNormal - secondVelocityAlongNormal >= 0f)
            {
                return;
            }

            firstGun.Velocity += normal * (secondVelocityAlongNormal - firstVelocityAlongNormal);
            secondGun.Velocity -= normal * (secondVelocityAlongNormal - firstVelocityAlongNormal);
        }
    }
}
