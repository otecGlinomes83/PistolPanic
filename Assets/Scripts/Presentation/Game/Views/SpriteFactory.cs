using System.Collections.Generic;
using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    public sealed class SpriteFactory
    {
        private const int SquareTextureSize = 4;

        private const int CircleTextureSize = 64;

        private const int SkinCacheKeyShift = 8;

        private readonly PixelArtFactory _pixelArtFactory = new PixelArtFactory();

        private Sprite _squareSprite;

        private Sprite _circleSprite;

        private Sprite _bulletSprite;

        private Sprite _floorSprite;

        private Sprite _wallSprite;

        private Sprite _obstacleSprite;

        private Sprite _panelSprite;

        private Sprite _accentSprite;

        private readonly Dictionary<long, Sprite> _gunSprites = new Dictionary<long, Sprite>();

        private readonly Dictionary<FlashShape, Sprite> _flashSprites = new Dictionary<FlashShape, Sprite>();

        public Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            Texture2D texture = new Texture2D(SquareTextureSize, SquareTextureSize, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[SquareTextureSize * SquareTextureSize];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);

            _squareSprite = Sprite.Create(texture, new Rect(0f, 0f, SquareTextureSize, SquareTextureSize), new Vector2(0.5f, 0.5f), SquareTextureSize);

            return _squareSprite;
        }

        public Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

            Texture2D texture = new Texture2D(CircleTextureSize, CircleTextureSize, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[CircleTextureSize * CircleTextureSize];
            float center = (CircleTextureSize - 1) * 0.5f;
            float radius = CircleTextureSize * 0.5f;
            float radiusSquared = radius * radius;

            for (int pixelY = 0; pixelY < CircleTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < CircleTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    bool isInsideCircle = distanceX * distanceX + distanceY * distanceY <= radiusSquared;
                    int pixelIndex = pixelY * CircleTextureSize + pixelX;

                    if (isInsideCircle)
                    {
                        pixels[pixelIndex] = Color.white;
                    }
                    else
                    {
                        pixels[pixelIndex] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);

            _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, CircleTextureSize, CircleTextureSize), new Vector2(0.5f, 0.5f), CircleTextureSize);

            return _circleSprite;
        }

        public Sprite GetGunSprite(FirePattern pattern, GunSkin skin)
        {
            long cacheKey = ((long)pattern << SkinCacheKeyShift) | (long)skin;

            Sprite cachedSprite;

            if (_gunSprites.TryGetValue(cacheKey, out cachedSprite))
            {
                return cachedSprite;
            }

            Sprite createdSprite = _pixelArtFactory.CreateGunSprite(pattern, skin);
            _gunSprites.Add(cacheKey, createdSprite);

            return createdSprite;
        }

        public Sprite GetBulletSprite()
        {
            if (_bulletSprite == null)
            {
                _bulletSprite = _pixelArtFactory.CreateBulletSprite();
            }

            return _bulletSprite;
        }

        public Sprite GetFlashSprite(FlashShape shape)
        {
            Sprite cachedSprite;

            if (_flashSprites.TryGetValue(shape, out cachedSprite))
            {
                return cachedSprite;
            }

            Sprite createdSprite = _pixelArtFactory.CreateFlashSprite(shape);
            _flashSprites.Add(shape, createdSprite);

            return createdSprite;
        }

        public Sprite GetFloorSprite()
        {
            if (_floorSprite == null)
            {
                _floorSprite = _pixelArtFactory.CreateFloorSprite();
            }

            return _floorSprite;
        }

        public Sprite GetWallSprite()
        {
            if (_wallSprite == null)
            {
                _wallSprite = _pixelArtFactory.CreateWallSprite();
            }

            return _wallSprite;
        }

        public Sprite GetObstacleSprite()
        {
            if (_obstacleSprite == null)
            {
                _obstacleSprite = _pixelArtFactory.CreateObstacleSprite();
            }

            return _obstacleSprite;
        }

        public Sprite GetPanelSprite()
        {
            if (_panelSprite == null)
            {
                _panelSprite = _pixelArtFactory.CreatePanelSprite();
            }

            return _panelSprite;
        }

        public Sprite GetAccentSprite()
        {
            if (_accentSprite == null)
            {
                _accentSprite = _pixelArtFactory.CreateAccentSprite();
            }

            return _accentSprite;
        }
    }
}
