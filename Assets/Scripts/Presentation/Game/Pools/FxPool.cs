using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class FxPool : MonoBehaviour
    {
        private readonly FlashFxView[] _views = new FlashFxView[12];

        private int _nextIndex;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        private void Start()
        {
            Sprite flashSprite = _spriteFactory.GetCircleSprite();

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i] = CreateView(flashSprite);
            }
        }

        public void PlayFlash(Vector3 position, Color color, float startScale, float endScale, float duration)
        {
            if (_views[0] == null)
            {
                return;
            }

            FlashFxView view = _views[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _views.Length;
            view.Play(position, color, startScale, endScale, duration);
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
