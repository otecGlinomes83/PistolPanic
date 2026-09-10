using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GunView : MonoBehaviour
    {
        private const float ViewScale = 0.8f;

        private SpriteRenderer _spriteRenderer;

        public void Initialize(Sprite gunSprite, Color gunColor)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = gunSprite;
            _spriteRenderer.color = gunColor;
            transform.localScale = new Vector3(ViewScale, ViewScale, 1f);
        }

        public void SetColor(Color gunColor)
        {
            _spriteRenderer.color = gunColor;
        }

        public void ApplySnapshot(GunSnapshot snapshot)
        {
            transform.position = new Vector3(snapshot.Position.x, snapshot.Position.y, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, snapshot.RotationDegrees);
        }
    }
}
