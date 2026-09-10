using UnityEngine;

namespace PistolPanic.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FlashFxView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private Color _flashColor;

        private float _startScale;

        private float _endScale;

        private float _durationSeconds;

        private float _elapsedSeconds;

        public void Initialize(Sprite flashSprite)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = flashSprite;
        }

        public void Play(Vector3 position, Color color, float startScale, float endScale, float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            gameObject.SetActive(true);
            transform.position = position;
            transform.localScale = new Vector3(startScale, startScale, 1f);
            _spriteRenderer.color = color;

            _flashColor = color;
            _startScale = startScale;
            _endScale = endScale;
            _durationSeconds = duration;
            _elapsedSeconds = 0f;
        }

        private void Update()
        {
            _elapsedSeconds += Time.deltaTime;

            if (_elapsedSeconds >= _durationSeconds)
            {
                gameObject.SetActive(false);

                return;
            }

            float progress = _elapsedSeconds / _durationSeconds;
            float scale = Mathf.Lerp(_startScale, _endScale, progress);
            Color fadedColor = _flashColor;
            fadedColor.a = 1f - progress;

            transform.localScale = new Vector3(scale, scale, 1f);
            _spriteRenderer.color = fadedColor;
        }
    }
}
