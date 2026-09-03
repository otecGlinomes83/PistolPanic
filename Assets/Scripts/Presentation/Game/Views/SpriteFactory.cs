using UnityEngine;

namespace PistolPanic.Presentation
{
    public sealed class SpriteFactory
    {
        private const int SquareTextureSize = 4;

        private const int CircleTextureSize = 64;

        private Sprite _squareSprite;

        private Sprite _circleSprite;

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

            for (int y = 0; y < CircleTextureSize; y++)
            {
                for (int x = 0; x < CircleTextureSize; x++)
                {
                    float distanceX = x - center;
                    float distanceY = y - center;
                    bool isInsideCircle = distanceX * distanceX + distanceY * distanceY <= radiusSquared;
                    int pixelIndex = y * CircleTextureSize + x;

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
    }
}
