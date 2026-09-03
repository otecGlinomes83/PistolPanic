using UnityEngine;

namespace PistolPanic.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BulletView : MonoBehaviour
    {
        public void Initialize(Sprite bulletSprite, Color bulletColor)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = bulletSprite;
            spriteRenderer.color = bulletColor;
        }

        public void Show(Vector2 position, float diameter)
        {
            gameObject.SetActive(true);
            transform.position = new Vector3(position.x, position.y, 0f);
            transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
