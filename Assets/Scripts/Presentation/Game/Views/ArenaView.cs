using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class ArenaView : MonoBehaviour
    {
        private const float WallThicknessUnits = 0.25f;

        [Inject]
        private readonly MapConfig _mapConfig = null;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        private readonly Color _wallColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        private readonly Color _obstacleColor = new Color(0.35f, 0.35f, 0.42f, 1f);

        private void Start()
        {
            BuildWalls();
            BuildObstacles();

            Debug.Log("ArenaView: built, obstacles=" + _mapConfig.Obstacles.Count);
        }

        private void BuildWalls()
        {
            Vector2 arenaSize = _mapConfig.ArenaSize;
            float halfWidth = arenaSize.x * 0.5f;
            float halfHeight = arenaSize.y * 0.5f;
            Sprite wallSprite = _spriteFactory.GetSquareSprite();
            Vector2 horizontalWallSize = new Vector2(arenaSize.x + WallThicknessUnits * 2f, WallThicknessUnits);
            Vector2 verticalWallSize = new Vector2(WallThicknessUnits, arenaSize.y);

            CreateWall(wallSprite, new Vector2(0f, halfHeight + WallThicknessUnits * 0.5f), horizontalWallSize);
            CreateWall(wallSprite, new Vector2(0f, -halfHeight - WallThicknessUnits * 0.5f), horizontalWallSize);
            CreateWall(wallSprite, new Vector2(halfWidth + WallThicknessUnits * 0.5f, 0f), verticalWallSize);
            CreateWall(wallSprite, new Vector2(-halfWidth - WallThicknessUnits * 0.5f, 0f), verticalWallSize);
        }

        private void CreateWall(Sprite wallSprite, Vector2 centerPosition, Vector2 size)
        {
            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.SetParent(transform, false);
            wallObject.transform.localPosition = new Vector3(centerPosition.x, centerPosition.y, 0f);
            wallObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer spriteRenderer = wallObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = wallSprite;
            spriteRenderer.color = _wallColor;
        }

        private void BuildObstacles()
        {
            Sprite obstacleSprite = _spriteFactory.GetCircleSprite();

            for (int i = 0; i < _mapConfig.Obstacles.Count; i++)
            {
                CreateObstacle(obstacleSprite, _mapConfig.Obstacles[i]);
            }
        }

        private void CreateObstacle(Sprite obstacleSprite, ObstacleData obstacleData)
        {
            GameObject obstacleObject = new GameObject("Obstacle");
            obstacleObject.transform.SetParent(transform, false);
            obstacleObject.transform.localPosition = new Vector3(obstacleData.Center.x, obstacleData.Center.y, 0f);
            obstacleObject.transform.localScale = new Vector3(obstacleData.Radius * 2f, obstacleData.Radius * 2f, 1f);

            SpriteRenderer spriteRenderer = obstacleObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = obstacleSprite;
            spriteRenderer.color = _obstacleColor;
        }
    }
}
