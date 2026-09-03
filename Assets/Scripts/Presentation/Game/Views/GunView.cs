using PistolPanic.Core;
using UnityEngine;

namespace PistolPanic.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GunView : MonoBehaviour
    {
        private const float ViewScale = 0.8f;

        public void Initialize(Sprite gunSprite, Color gunColor)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = gunSprite;
            spriteRenderer.color = gunColor;
            transform.localScale = new Vector3(ViewScale, ViewScale, 1f);
        }

        public void SetColor(Color gunColor)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = gunColor;
        }

        public void ApplySnapshot(GunSnapshot snapshot)
        {
            transform.position = new Vector3(snapshot.Position.x, snapshot.Position.y, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, snapshot.RotationDegrees);
        }
    }
}
