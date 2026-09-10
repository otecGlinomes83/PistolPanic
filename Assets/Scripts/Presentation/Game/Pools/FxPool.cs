using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class FxPool : MonoBehaviour
    {
        private const int ViewsPerShape = 4;

        private readonly FlashFxView[] _starViews = new FlashFxView[ViewsPerShape];

        private readonly FlashFxView[] _glowViews = new FlashFxView[ViewsPerShape];

        private readonly FlashFxView[] _ringViews = new FlashFxView[ViewsPerShape];

        private int _nextStarIndex;

        private int _nextGlowIndex;

        private int _nextRingIndex;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        private void Start()
        {
            Sprite starSprite = _spriteFactory.GetFlashSprite(FlashShape.Star);
            Sprite glowSprite = _spriteFactory.GetFlashSprite(FlashShape.Glow);
            Sprite ringSprite = _spriteFactory.GetFlashSprite(FlashShape.Ring);

            for (int i = 0; i < ViewsPerShape; i++)
            {
                _starViews[i] = CreateView(starSprite);
                _glowViews[i] = CreateView(glowSprite);
                _ringViews[i] = CreateView(ringSprite);
            }
        }

        public void PlayFlash(Vector3 position, Color color, float startScale, float endScale, float duration)
        {
            PlayFlash(position, color, startScale, endScale, duration, FlashShape.Glow);
        }

        public void PlayFlash(Vector3 position, Color color, float startScale, float endScale, float duration, FlashShape shape)
        {
            FlashFxView view = TakeView(shape);

            if (view == null)
            {
                return;
            }

            view.Play(position, color, startScale, endScale, duration);
        }

        private FlashFxView TakeView(FlashShape shape)
        {
            int shapeValue = (int)shape;

            if (shapeValue == (int)FlashShape.Star)
            {
                return TakeFromArray(_starViews, ref _nextStarIndex);
            }

            if (shapeValue == (int)FlashShape.Ring)
            {
                return TakeFromArray(_ringViews, ref _nextRingIndex);
            }

            return TakeFromArray(_glowViews, ref _nextGlowIndex);
        }

        private FlashFxView TakeFromArray(FlashFxView[] views, ref int nextIndex)
        {
            if (views[0] == null)
            {
                return null;
            }

            FlashFxView view = views[nextIndex];
            nextIndex = (nextIndex + 1) % views.Length;

            return view;
        }

        private FlashFxView CreateView(Sprite flashSprite)
        {
            GameObject viewObject = new GameObject("FlashFx");
            viewObject.transform.SetParent(transform, false);
            viewObject.SetActive(false);

            FlashFxView view = viewObject.AddComponent<FlashFxView>();
            view.Initialize(flashSprite);

            return view;
        }
    }
}
