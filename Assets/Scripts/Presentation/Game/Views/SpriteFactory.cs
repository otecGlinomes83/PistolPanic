using System.Collections.Generic;
using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    public sealed class SpriteFactory
    {
        private const int SquareTextureSize = 4;

        private const int CircleTextureSize = 64;

        private const int GunTextureWidth = 128;

        private const int GunTextureHeight = 64;

        private const float GunPixelsPerUnit = 64f;

        private Sprite _squareSprite;

        private Sprite _circleSprite;

        private readonly Dictionary<FirePattern, Sprite> _gunSprites = new Dictionary<FirePattern, Sprite>();

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

        public Sprite GetGunSprite(FirePattern pattern)
        {
            Sprite cachedSprite;

            if (_gunSprites.TryGetValue(pattern, out cachedSprite))
            {
                return cachedSprite;
            }

            Sprite createdSprite = CreateGunSprite(pattern);
            _gunSprites.Add(pattern, createdSprite);

            return createdSprite;
        }

        private Sprite CreateGunSprite(FirePattern pattern)
        {
            int patternValue = (int)pattern;

            if (patternValue == (int)FirePattern.Burst)
            {
                return CreateBurstGunSprite();
            }

            if (patternValue == (int)FirePattern.Shotgun)
            {
                return CreateShotgunGunSprite();
            }

            return CreateSingleGunSprite();
        }

        private Sprite CreateSingleGunSprite()
        {
            Color[] pixels = CreateClearGunPixels();

            FillRect(pixels, GunTextureWidth, 16, 24, 80, 48);
            FillRect(pixels, GunTextureWidth, 80, 32, 112, 44);
            FillRect(pixels, GunTextureWidth, 24, 8, 44, 24);

            return CreateSpriteFromGunPixels(pixels);
        }

        private Sprite CreateBurstGunSprite()
        {
            Color[] pixels = CreateClearGunPixels();

            FillRect(pixels, GunTextureWidth, 8, 26, 104, 46);
            FillRect(pixels, GunTextureWidth, 104, 32, 124, 42);
            FillRect(pixels, GunTextureWidth, 48, 10, 64, 26);
            FillRect(pixels, GunTextureWidth, 8, 20, 22, 34);

            return CreateSpriteFromGunPixels(pixels);
        }

        private Sprite CreateShotgunGunSprite()
        {
            Color[] pixels = CreateClearGunPixels();

            FillRect(pixels, GunTextureWidth, 8, 28, 120, 44);
            FillRect(pixels, GunTextureWidth, 56, 20, 80, 28);
            FillRect(pixels, GunTextureWidth, 8, 36, 24, 52);

            return CreateSpriteFromGunPixels(pixels);
        }

        private Color[] CreateClearGunPixels()
        {
            Color[] pixels = new Color[GunTextureWidth * GunTextureHeight];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            return pixels;
        }

        private void FillRect(Color[] pixels, int textureWidth, int x0, int y0, int x1, int y1)
        {
            for (int pixelY = y0; pixelY <= y1; pixelY++)
            {
                for (int pixelX = x0; pixelX <= x1; pixelX++)
                {
                    pixels[pixelY * textureWidth + pixelX] = Color.white;
                }
            }
        }

        private Sprite CreateSpriteFromGunPixels(Color[] pixels)
        {
            Texture2D texture = new Texture2D(GunTextureWidth, GunTextureHeight, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply(true, false);

            return Sprite.Create(texture, new Rect(0f, 0f, GunTextureWidth, GunTextureHeight), new Vector2(0.5f, 0.5f), GunPixelsPerUnit);
        }
    }
}
