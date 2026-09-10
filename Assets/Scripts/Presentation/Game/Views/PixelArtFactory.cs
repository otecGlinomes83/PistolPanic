using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    public sealed class PixelArtFactory
    {
        private const int GunTextureWidth = 48;

        private const int GunTextureHeight = 24;

        private const float GunPixelsPerUnit = 48f;

        private const int BulletTextureSize = 16;

        private const float BulletPixelsPerUnit = 16f;

        private const int StarTextureSize = 24;

        private const float StarPixelsPerUnit = 24f;

        private const int RingTextureSize = 48;

        private const float RingPixelsPerUnit = 24f;

        private const int FloorTextureWidth = 96;

        private const int FloorTextureHeight = 160;

        private const float FloorPixelsPerUnit = 16f;

        private const int WallTextureSize = 16;

        private const float WallPixelsPerUnit = 16f;

        private const int ObstacleTextureSize = 32;

        private const float ObstaclePixelsPerUnit = 32f;

        private const int PanelTextureSize = 24;

        private const float PanelPixelsPerUnit = 24f;

        private const int PanelBorderPx = 8;

        private const int OutlineThickness = 2;

        private const uint OutlineColor = 0xFF14181Du;

        private const uint PanelFillColor = 0xE61A2130u;

        private const uint AccentFillColor = 0xFFE6A23Cu;

        private readonly GunPalette _playerSteelPalette = new GunPalette(
            0x3A4653u,
            0x5A6B7Du,
            0x232B34u,
            0x67D07Cu,
            0x8FE79Au,
            0x3F8F52u,
            0x6B4A32u,
            0x8A6647u,
            0x4A3221u);

        private readonly GunPalette _enemyRedPalette = new GunPalette(
            0x463A40u,
            0x67565Cu,
            0x2A2226u,
            0xE05548u,
            0xF08A7Au,
            0x9E3327u,
            0x6B4A32u,
            0x8A6647u,
            0x4A3221u);

        private readonly GunPalette _enemyOrangePalette = new GunPalette(
            0x47413Au,
            0x686055u,
            0x2B2620u,
            0xE6922Eu,
            0xF2B662u,
            0xA3611Bu,
            0x6B4A32u,
            0x8A6647u,
            0x4A3221u);

        private readonly GunPalette _enemyPurplePalette = new GunPalette(
            0x423C4Eu,
            0x625B72u,
            0x27222Fu,
            0x9A5FD0u,
            0xBE8CE8u,
            0x653A8Fu,
            0x6B4A32u,
            0x8A6647u,
            0x4A3221u);

        public Sprite CreateGunSprite(FirePattern pattern, GunSkin skin)
        {
            uint[] pixels = CreateClearPixels(GunTextureWidth, GunTextureHeight);
            GunPalette palette = ResolvePalette(skin);
            int patternValue = (int)pattern;

            if (patternValue == (int)FirePattern.Burst)
            {
                DrawAutomatParts(pixels, GunTextureWidth, GunTextureHeight, palette, true);
                DrawAutomatParts(pixels, GunTextureWidth, GunTextureHeight, palette, false);
            }
            else if (patternValue == (int)FirePattern.Shotgun)
            {
                DrawShotgunParts(pixels, GunTextureWidth, GunTextureHeight, palette, true);
                DrawShotgunParts(pixels, GunTextureWidth, GunTextureHeight, palette, false);
            }
            else
            {
                DrawPistolParts(pixels, GunTextureWidth, GunTextureHeight, palette, true);
                DrawPistolParts(pixels, GunTextureWidth, GunTextureHeight, palette, false);
            }

            return BakeSprite("Gun_" + pattern + "_" + skin, GunTextureWidth, GunTextureHeight, pixels, GunPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        public Sprite CreateBulletSprite()
        {
            uint[] pixels = CreateClearPixels(BulletTextureSize, BulletTextureSize);
            float center = (BulletTextureSize - 1) * 0.5f;

            for (int pixelY = 0; pixelY < BulletTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < BulletTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                    if (distance <= 1.5f)
                    {
                        SetPixel(pixels, BulletTextureSize, BulletTextureSize, pixelX, pixelY, 0xFFFFFFFFu);

                        continue;
                    }

                    int pixelAlpha = ResolveTracerAlpha(distance);

                    if (pixelAlpha <= 0)
                    {
                        continue;
                    }

                    SetPixel(pixels, BulletTextureSize, BulletTextureSize, pixelX, pixelY, ComposeAlpha(0xFFE9A8u, pixelAlpha));
                }
            }

            return BakeSprite("BulletTracer", BulletTextureSize, BulletTextureSize, pixels, BulletPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        public Sprite CreateFlashSprite(FlashShape shape)
        {
            int shapeValue = (int)shape;

            if (shapeValue == (int)FlashShape.Star)
            {
                return CreateStarFlashSprite();
            }

            if (shapeValue == (int)FlashShape.Ring)
            {
                return CreateRingFlashSprite();
            }

            return CreateGlowFlashSprite();
        }

        public Sprite CreateFloorSprite()
        {
            uint[] pixels = CreateClearPixels(FloorTextureWidth, FloorTextureHeight);

            FillVerticalGradient(pixels, FloorTextureWidth, FloorTextureHeight, 0, 0, FloorTextureWidth - 1, FloorTextureHeight - 1, 0xFF10141Cu, 0xFF0B0E14u);

            for (int gridX = 8; gridX < FloorTextureWidth; gridX += 8)
            {
                FillRect(pixels, FloorTextureWidth, FloorTextureHeight, gridX, 0, gridX, FloorTextureHeight - 1, 0xFF1A2130u);
            }

            for (int gridY = 8; gridY < FloorTextureHeight; gridY += 8)
            {
                FillRect(pixels, FloorTextureWidth, FloorTextureHeight, 0, gridY, FloorTextureWidth - 1, gridY, 0xFF1A2130u);
            }

            OutlineRect(pixels, FloorTextureWidth, FloorTextureHeight, 0, 0, FloorTextureWidth - 1, FloorTextureHeight - 1, 0xFF070A0Fu);
            OutlineRect(pixels, FloorTextureWidth, FloorTextureHeight, 1, 1, FloorTextureWidth - 2, FloorTextureHeight - 2, 0xFF070A0Fu);
            OutlineRect(pixels, FloorTextureWidth, FloorTextureHeight, 2, 2, FloorTextureWidth - 3, FloorTextureHeight - 3, 0xFF070A0Fu);

            return BakeSprite("ArenaFloor", FloorTextureWidth, FloorTextureHeight, pixels, FloorPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        public Sprite CreateWallSprite()
        {
            uint[] pixels = CreateClearPixels(WallTextureSize, WallTextureSize);

            FillRect(pixels, WallTextureSize, WallTextureSize, 0, 0, WallTextureSize - 1, WallTextureSize - 1, 0xFF2C3644u);
            FillRect(pixels, WallTextureSize, WallTextureSize, 1, 1, WallTextureSize - 2, WallTextureSize - 2, 0xFF3A4653u);

            FillRect(pixels, WallTextureSize, WallTextureSize, 1, WallTextureSize - 2, WallTextureSize - 2, WallTextureSize - 2, 0xFF46556Au);
            FillRect(pixels, WallTextureSize, WallTextureSize, 1, 1, 1, WallTextureSize - 2, 0xFF46556Au);

            FillRect(pixels, WallTextureSize, WallTextureSize, 1, 1, WallTextureSize - 2, 1, 0xFF1E2530u);
            FillRect(pixels, WallTextureSize, WallTextureSize, WallTextureSize - 2, 1, WallTextureSize - 2, WallTextureSize - 2, 0xFF1E2530u);

            SetPixel(pixels, WallTextureSize, WallTextureSize, 4, WallTextureSize - 5, 0xFF151B24u);
            SetPixel(pixels, WallTextureSize, WallTextureSize, WallTextureSize - 5, 4, 0xFF151B24u);

            OutlineRect(pixels, WallTextureSize, WallTextureSize, 0, 0, WallTextureSize - 1, WallTextureSize - 1, OutlineColor);

            return BakeSprite("WallSegment", WallTextureSize, WallTextureSize, pixels, WallPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        public Sprite CreateObstacleSprite()
        {
            uint[] pixels = CreateClearPixels(ObstacleTextureSize, ObstacleTextureSize);
            float center = (ObstacleTextureSize - 1) * 0.5f;

            for (int pixelY = 0; pixelY < ObstacleTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < ObstacleTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                    if (distance > 15f)
                    {
                        continue;
                    }

                    float shade = (distanceX + distanceY) / 15.5f;

                    if (shade < -0.3f)
                    {
                        SetPixel(pixels, ObstacleTextureSize, ObstacleTextureSize, pixelX, pixelY, 0xFF5A6B7Du);
                    }
                    else if (shade > 0.35f)
                    {
                        SetPixel(pixels, ObstacleTextureSize, ObstacleTextureSize, pixelX, pixelY, 0xFF232B34u);
                    }
                    else
                    {
                        SetPixel(pixels, ObstacleTextureSize, ObstacleTextureSize, pixelX, pixelY, 0xFF3A4653u);
                    }
                }
            }

            FillEllipse(pixels, ObstacleTextureSize, ObstacleTextureSize, center - 4.5f, center + 4.5f, 1.6f, 1.6f, 0xFFB8C4D4u);

            for (int pixelY = 0; pixelY < ObstacleTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < ObstacleTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                    if (distance > 13.6f && distance <= 15f)
                    {
                        SetPixel(pixels, ObstacleTextureSize, ObstacleTextureSize, pixelX, pixelY, OutlineColor);
                    }
                }
            }

            return BakeSprite("ArenaObstacle", ObstacleTextureSize, ObstacleTextureSize, pixels, ObstaclePixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        public Sprite CreatePanelSprite()
        {
            return CreateRoundedPanelSprite("UiPanel", PanelFillColor);
        }

        public Sprite CreateAccentSprite()
        {
            return CreateRoundedPanelSprite("UiAccentPanel", AccentFillColor);
        }

        private Sprite CreateRoundedPanelSprite(string spriteName, uint fillColor)
        {
            uint[] pixels = CreateClearPixels(PanelTextureSize, PanelTextureSize);

            for (int pixelY = 0; pixelY < PanelTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < PanelTextureSize; pixelX++)
                {
                    if (IsInsideRoundedRect(pixelX, pixelY, PanelTextureSize, PanelTextureSize, 4))
                    {
                        SetPixel(pixels, PanelTextureSize, PanelTextureSize, pixelX, pixelY, OutlineColor);
                    }
                }
            }

            for (int pixelY = 0; pixelY < PanelTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < PanelTextureSize; pixelX++)
                {
                    bool isInsideEroded = IsInsideRoundedRect(pixelX - 2, pixelY - 2, PanelTextureSize - 4, PanelTextureSize - 4, 2);

                    if (isInsideEroded)
                    {
                        SetPixel(pixels, PanelTextureSize, PanelTextureSize, pixelX, pixelY, fillColor);
                    }
                }
            }

            return BakeSprite(spriteName, PanelTextureSize, PanelTextureSize, pixels, PanelPixelsPerUnit, new Vector2(0.5f, 0.5f), PanelBorderPx);
        }

        private bool IsInsideRoundedRect(int pixelX, int pixelY, int width, int height, int radius)
        {
            if (pixelX < 0 || pixelY < 0 || pixelX >= width || pixelY >= height)
            {
                return false;
            }

            int clampMin = radius;
            int clampMaxX = width - 1 - radius;
            int clampMaxY = height - 1 - radius;

            int nearestX = pixelX;

            if (nearestX < clampMin)
            {
                nearestX = clampMin;
            }

            if (nearestX > clampMaxX)
            {
                nearestX = clampMaxX;
            }

            int nearestY = pixelY;

            if (nearestY < clampMin)
            {
                nearestY = clampMin;
            }

            if (nearestY > clampMaxY)
            {
                nearestY = clampMaxY;
            }

            float offsetX = pixelX - nearestX;
            float offsetY = pixelY - nearestY;

            return offsetX * offsetX + offsetY * offsetY <= radius * radius;
        }

        private Sprite CreateStarFlashSprite()
        {
            uint[] pixels = CreateClearPixels(StarTextureSize, StarTextureSize);
            float center = (StarTextureSize - 1) * 0.5f;

            for (int pixelX = 0; pixelX < StarTextureSize; pixelX++)
            {
                float spikeAlpha = 255f * (1f - Mathf.Abs(pixelX - center) / (center + 1f));

                SetPixel(pixels, StarTextureSize, StarTextureSize, pixelX, 11, ComposeAlpha(0xFFFFF4D6u, (int)spikeAlpha));
                SetPixel(pixels, StarTextureSize, StarTextureSize, pixelX, 12, ComposeAlpha(0xFFFFF4D6u, (int)spikeAlpha));
            }

            for (int pixelY = 0; pixelY < StarTextureSize; pixelY++)
            {
                float spikeAlpha = 255f * (1f - Mathf.Abs(pixelY - center) / (center + 1f));

                SetPixel(pixels, StarTextureSize, StarTextureSize, 11, pixelY, ComposeAlpha(0xFFFFF4D6u, (int)spikeAlpha));
                SetPixel(pixels, StarTextureSize, StarTextureSize, 12, pixelY, ComposeAlpha(0xFFFFF4D6u, (int)spikeAlpha));
            }

            for (int step = 1; step <= 6; step++)
            {
                float diagonalAlpha = 220f * (1f - step / 7f);
                uint diagonalColor = ComposeAlpha(0xFFFFF4D6u, (int)diagonalAlpha);

                SetPixel(pixels, StarTextureSize, StarTextureSize, 11 - step, 11 - step, diagonalColor);
                SetPixel(pixels, StarTextureSize, StarTextureSize, 12 + step, 11 - step, diagonalColor);
                SetPixel(pixels, StarTextureSize, StarTextureSize, 11 - step, 12 + step, diagonalColor);
                SetPixel(pixels, StarTextureSize, StarTextureSize, 12 + step, 12 + step, diagonalColor);
            }

            FillRect(pixels, StarTextureSize, StarTextureSize, 10, 10, 13, 13, 0xFFFFFFFFu);

            return BakeSprite("FlashStar", StarTextureSize, StarTextureSize, pixels, StarPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        private Sprite CreateGlowFlashSprite()
        {
            uint[] pixels = CreateClearPixels(StarTextureSize, StarTextureSize);
            float center = (StarTextureSize - 1) * 0.5f;

            for (int pixelY = 0; pixelY < StarTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < StarTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
                    float glowAlpha = 255f * (1f - distance / (center + 0.5f));

                    if (glowAlpha <= 0f)
                    {
                        continue;
                    }

                    SetPixel(pixels, StarTextureSize, StarTextureSize, pixelX, pixelY, ComposeAlpha(0xFFFDF2DCu, (int)glowAlpha));
                }
            }

            return BakeSprite("FlashGlow", StarTextureSize, StarTextureSize, pixels, StarPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        private Sprite CreateRingFlashSprite()
        {
            uint[] pixels = CreateClearPixels(RingTextureSize, RingTextureSize);
            float center = (RingTextureSize - 1) * 0.5f;

            for (int pixelY = 0; pixelY < RingTextureSize; pixelY++)
            {
                for (int pixelX = 0; pixelX < RingTextureSize; pixelX++)
                {
                    float distanceX = pixelX - center;
                    float distanceY = pixelY - center;
                    float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
                    float ringAlpha = ResolveRingAlpha(distance);

                    if (ringAlpha <= 0f)
                    {
                        continue;
                    }

                    SetPixel(pixels, RingTextureSize, RingTextureSize, pixelX, pixelY, ComposeAlpha(0xFFFFF0DCu, (int)ringAlpha));
                }
            }

            return BakeSprite("FlashRing", RingTextureSize, RingTextureSize, pixels, RingPixelsPerUnit, new Vector2(0.5f, 0.5f), 0);
        }

        private float ResolveRingAlpha(float distance)
        {
            if (distance < 16f || distance > 25f)
            {
                return 0f;
            }

            if (distance < 18f)
            {
                return 255f * (distance - 16f) / 2f;
            }

            if (distance <= 23f)
            {
                return 255f;
            }

            return 255f * (1f - (distance - 23f) / 2f);
        }

        private int ResolveTracerAlpha(float distance)
        {
            if (distance <= 3.5f)
            {
                float glowAlpha = 200f - 55f * (distance - 1.5f);

                return (int)glowAlpha;
            }

            if (distance > 7.5f)
            {
                return 0;
            }

            float haloAlpha = 90f * (1f - (distance - 3.5f) / 4f);

            return (int)haloAlpha;
        }

        private GunPalette ResolvePalette(GunSkin skin)
        {
            int skinValue = (int)skin;

            if (skinValue == (int)GunSkin.EnemyRed)
            {
                return _enemyRedPalette;
            }

            if (skinValue == (int)GunSkin.EnemyOrange)
            {
                return _enemyOrangePalette;
            }

            if (skinValue == (int)GunSkin.EnemyPurple)
            {
                return _enemyPurplePalette;
            }

            return _playerSteelPalette;
        }

        private void DrawPistolParts(uint[] pixels, int width, int height, GunPalette palette, bool drawOutlineOnly)
        {
            DrawPart(pixels, width, height, drawOutlineOnly, 10, 13, 38, 18, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 10, 15, 19, 16, palette.AccentLight, palette.Accent, palette.AccentDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 35, 19, 36, 20, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 11, 19, 12, 20, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 12, 10, 33, 12, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 20, 6, 27, 7, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 26, 8, 27, 10, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 20, 8, 21, 10, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 13, 6, 18, 10, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 10, 2, 15, 6, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 39, 15, 42, 16, palette.BodyLight, palette.Body, palette.BodyDark);

            if (drawOutlineOnly)
            {
                return;
            }

            SetPixel(pixels, width, height, 15, 8, palette.BodyDark);
            SetPixel(pixels, width, height, 12, 4, palette.BodyDark);
            SetPixel(pixels, width, height, 42, 15, palette.BodyDark);
            SetPixel(pixels, width, height, 42, 16, palette.BodyDark);
        }

        private void DrawAutomatParts(uint[] pixels, int width, int height, GunPalette palette, bool drawOutlineOnly)
        {
            DrawPart(pixels, width, height, drawOutlineOnly, 9, 12, 35, 18, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 36, 14, 45, 17, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 43, 18, 44, 19, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 29, 18, 40, 19, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 18, 9, 23, 12, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 19, 5, 24, 8, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 20, 1, 25, 4, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 10, 8, 15, 11, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 9, 4, 14, 7, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 2, 12, 9, 17, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 30, 10, 33, 11, palette.AccentLight, palette.Accent, palette.AccentDark);

            if (drawOutlineOnly)
            {
                return;
            }

            FillRect(pixels, width, height, 4, 14, 7, 15, 0x00000000u);
            SetPixel(pixels, width, height, 45, 15, palette.BodyDark);
            SetPixel(pixels, width, height, 45, 16, palette.BodyDark);
            SetPixel(pixels, width, height, 44, 15, palette.BodyDark);
        }

        private void DrawShotgunParts(uint[] pixels, int width, int height, GunPalette palette, bool drawOutlineOnly)
        {
            DrawPart(pixels, width, height, drawOutlineOnly, 8, 15, 45, 17, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 10, 12, 22, 17, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 13, 14, 17, 15, palette.AccentLight, palette.Accent, palette.AccentDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 24, 12, 36, 14, palette.WoodLight, palette.Wood, palette.WoodDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 14, 7, 19, 8, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 14, 9, 15, 11, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 18, 9, 19, 11, palette.BodyLight, palette.Body, palette.BodyDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 4, 12, 10, 16, palette.WoodLight, palette.Wood, palette.WoodDark);
            DrawPart(pixels, width, height, drawOutlineOnly, 2, 8, 6, 11, palette.WoodLight, palette.Wood, palette.WoodDark);

            if (drawOutlineOnly)
            {
                return;
            }

            FillRect(pixels, width, height, 28, 12, 28, 14, palette.WoodDark);
            FillRect(pixels, width, height, 32, 12, 32, 14, palette.WoodDark);
            SetPixel(pixels, width, height, 45, 16, palette.BodyDark);
            SetPixel(pixels, width, height, 45, 15, palette.BodyDark);
        }

        private void DrawPart(uint[] pixels, int width, int height, bool drawOutlineOnly, int x0, int y0, int x1, int y1, uint lightColor, uint bodyColor, uint darkColor)
        {
            if (drawOutlineOnly)
            {
                FillRect(pixels, width, height, x0 - OutlineThickness, y0 - OutlineThickness, x1 + OutlineThickness, y1 + OutlineThickness, OutlineColor);

                return;
            }

            FillShadedRect(pixels, width, height, x0, y0, x1, y1, lightColor, bodyColor, darkColor);
        }

        private void FillShadedRect(uint[] pixels, int width, int height, int x0, int y0, int x1, int y1, uint lightColor, uint bodyColor, uint darkColor)
        {
            bool isSingleRow = y0 == y1;

            for (int pixelY = y0; pixelY <= y1; pixelY++)
            {
                uint rowColor = bodyColor;

                if (isSingleRow == false && pixelY == y1)
                {
                    rowColor = lightColor;
                }
                else if (isSingleRow == false && pixelY == y0)
                {
                    rowColor = darkColor;
                }

                FillRect(pixels, width, height, x0, pixelY, x1, pixelY, rowColor);
            }
        }

        private uint[] CreateClearPixels(int width, int height)
        {
            uint[] pixels = new uint[width * height];

            return pixels;
        }

        private void SetPixel(uint[] pixels, int width, int height, int x, int y, uint color)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return;
            }

            pixels[y * width + x] = color;
        }

        private void FillRect(uint[] pixels, int width, int height, int x0, int y0, int x1, int y1, uint color)
        {
            for (int pixelY = y0; pixelY <= y1; pixelY++)
            {
                for (int pixelX = x0; pixelX <= x1; pixelX++)
                {
                    SetPixel(pixels, width, height, pixelX, pixelY, color);
                }
            }
        }

        private void OutlineRect(uint[] pixels, int width, int height, int x0, int y0, int x1, int y1, uint color)
        {
            FillRect(pixels, width, height, x0, y0, x1, y0, color);
            FillRect(pixels, width, height, x0, y1, x1, y1, color);
            FillRect(pixels, width, height, x0, y0, x0, y1, color);
            FillRect(pixels, width, height, x1, y0, x1, y1, color);
        }

        private void FillEllipse(uint[] pixels, int width, int height, float centerX, float centerY, float radiusX, float radiusY, uint color)
        {
            int minX = Mathf.FloorToInt(centerX - radiusX);
            int maxX = Mathf.CeilToInt(centerX + radiusX);
            int minY = Mathf.FloorToInt(centerY - radiusY);
            int maxY = Mathf.CeilToInt(centerY + radiusY);

            for (int pixelY = minY; pixelY <= maxY; pixelY++)
            {
                for (int pixelX = minX; pixelX <= maxX; pixelX++)
                {
                    float normalizedX = (pixelX - centerX) / radiusX;
                    float normalizedY = (pixelY - centerY) / radiusY;

                    if (normalizedX * normalizedX + normalizedY * normalizedY <= 1f)
                    {
                        SetPixel(pixels, width, height, pixelX, pixelY, color);
                    }
                }
            }
        }

        private void FillVerticalGradient(uint[] pixels, int width, int height, int x0, int y0, int x1, int y1, uint topColor, uint bottomColor)
        {
            float rowSpan = y1 - y0;

            for (int pixelY = y0; pixelY <= y1; pixelY++)
            {
                float progress = (pixelY - y0) / rowSpan;
                uint rowColor = LerpColor(bottomColor, topColor, progress);

                FillRect(pixels, width, height, x0, pixelY, x1, pixelY, rowColor);
            }
        }

        private void FillHorizontalGradient(uint[] pixels, int width, int height, int x0, int y0, int x1, int y1, uint leftColor, uint rightColor)
        {
            float columnSpan = x1 - x0;

            for (int pixelX = x0; pixelX <= x1; pixelX++)
            {
                float progress = (pixelX - x0) / columnSpan;
                uint columnColor = LerpColor(leftColor, rightColor, progress);

                FillRect(pixels, width, height, pixelX, y0, pixelX, y1, columnColor);
            }
        }

        private uint LerpColor(uint colorA, uint colorB, float progress)
        {
            int alphaA = (int)((colorA >> 24) & 0xFFu);
            int redA = (int)((colorA >> 16) & 0xFFu);
            int greenA = (int)((colorA >> 8) & 0xFFu);
            int blueA = (int)(colorA & 0xFFu);

            int alphaB = (int)((colorB >> 24) & 0xFFu);
            int redB = (int)((colorB >> 16) & 0xFFu);
            int greenB = (int)((colorB >> 8) & 0xFFu);
            int blueB = (int)(colorB & 0xFFu);

            int alpha = alphaA + Mathf.RoundToInt((alphaB - alphaA) * progress);
            int red = redA + Mathf.RoundToInt((redB - redA) * progress);
            int green = greenA + Mathf.RoundToInt((greenB - greenA) * progress);
            int blue = blueA + Mathf.RoundToInt((blueB - blueA) * progress);

            return (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
        }

        private uint ComposeAlpha(uint rgbColor, int alpha)
        {
            return (rgbColor & 0x00FFFFFFu) | ((uint)alpha << 24);
        }

        private Sprite BakeSprite(string spriteName, int width, int height, uint[] pixels, float pixelsPerUnit, Vector2 pivot, int spriteBorderPx)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] colors = new Color32[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                byte alphaByte = (byte)((pixels[i] >> 24) & 0xFFu);
                byte redByte = (byte)((pixels[i] >> 16) & 0xFFu);
                byte greenByte = (byte)((pixels[i] >> 8) & 0xFFu);
                byte blueByte = (byte)(pixels[i] & 0xFFu);

                colors[i] = new Color32(redByte, greenByte, blueByte, alphaByte);
            }

            texture.SetPixels32(colors);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(true, false);

            Vector4 spriteBorder = new Vector4(spriteBorderPx, spriteBorderPx, spriteBorderPx, spriteBorderPx);
            Sprite createdSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), pivot, pixelsPerUnit, 0u, SpriteMeshType.FullRect, spriteBorder);
            createdSprite.name = spriteName;

            return createdSprite;
        }
    }
}
