using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class ArenaView : MonoBehaviour
    {
        private const float WallThicknessUnits = 0.25f;

        private const float FloorSpriteWidthUnits = 6f;

        private const float FloorSpriteHeightUnits = 10f;

        private const int FloorSortingOrder = -10;

        private const float CameraBackgroundRed = 0.043f;

        private const float CameraBackgroundGreen = 0.055f;

        private const float CameraBackgroundBlue = 0.078f;

        [Inject]
        private readonly MapConfig _mapConfig = null;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        private void Start()
        {
            CreateFloor();
            BuildWalls();
            BuildObstacles();
            ApplyCameraBackground();

            Debug.Log("ArenaView: built, obstacles=" + _mapConfig.Obstacles.Count);
        }

        private void CreateFloor()
        {
            Vector2 arenaSize = _mapConfig.ArenaSize;

            GameObject floorObject = new GameObject("Floor");
            floorObject.transform.SetParent(transform, false);
            floorObject.transform.localPosition = new Vector3(0f, 0f, 1f);
            floorObject.transform.localScale = new Vector3(arenaSize.x / FloorSpriteWidthUnits, arenaSize.y / FloorSpriteHeightUnits, 1f);

            SpriteRenderer spriteRenderer = floorObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _spriteFactory.GetFloorSprite();
            spriteRenderer.sortingOrder = FloorSortingOrder;
        }

        private void BuildWalls()
        {
            Vector2 arenaSize = _mapConfig.ArenaSize;
            float halfWidth = arenaSize.x * 0.5f;
            float halfHeight = arenaSize.y * 0.5f;
            float horizontalWallLength = arenaSize.x + WallThicknessUnits * 2f;
            float verticalWallLength = arenaSize.y;

            CreateWall(new Vector2(0f, halfHeight - WallThicknessUnits * 0.5f), horizontalWallLength, true);
            CreateWall(new Vector2(0f, -halfHeight + WallThicknessUnits * 0.5f), horizontalWallLength, true);
            CreateWall(new Vector2(halfWidth - WallThicknessUnits * 0.5f, 0f), verticalWallLength, false);
            CreateWall(new Vector2(-halfWidth + WallThicknessUnits * 0.5f, 0f), verticalWallLength, false);
        }

        private void CreateWall(Vector2 centerPosition, float wallLength, bool isHorizontal)
        {
            Sprite wallSprite = _spriteFactory.GetWallSprite();
            int segmentCount = Mathf.CeilToInt(wallLength);
            float segmentLength = wallLength / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float offset = (i + 0.5f) * segmentLength - wallLength * 0.5f;

                CreateWallSegment(wallSprite, centerPosition, offset, segmentLength, isHorizontal);
            }
        }

        private void CreateWallSegment(Sprite wallSprite, Vector2 centerPosition, float offset, float segmentLength, bool isHorizontal)
        {
            float positionX;
            float positionY;
            float scaleX;
            float scaleY;

            if (isHorizontal)
            {
                positionX = centerPosition.x + offset;
                positionY = centerPosition.y;
                scaleX = segmentLength;
                scaleY = WallThicknessUnits;
            }
            else
            {
                positionX = centerPosition.x;
                positionY = centerPosition.y + offset;
                scaleX = WallThicknessUnits;
                scaleY = segmentLength;
            }

            GameObject segmentObject = new GameObject("WallSegment");
            segmentObject.transform.SetParent(transform, false);
            segmentObject.transform.localPosition = new Vector3(positionX, positionY, 0f);
            segmentObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            SpriteRenderer spriteRenderer = segmentObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = wallSprite;
        }

        private void BuildObstacles()
        {
            Sprite obstacleSprite = _spriteFactory.GetObstacleSprite();

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
        }

        private void ApplyCameraBackground()
        {
            Color backgroundColor = new Color(CameraBackgroundRed, CameraBackgroundGreen, CameraBackgroundBlue, 1f);
            Camera[] sceneCameras = Camera.allCameras;

            for (int i = 0; i < sceneCameras.Length; i++)
            {
                sceneCameras[i].backgroundColor = backgroundColor;
            }
        }
    }
}
