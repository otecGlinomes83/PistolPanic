using System.Collections.Generic;
using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class BulletPool : MonoBehaviour, IBulletPool
    {
        private readonly Stack<BulletView> _freeViews = new Stack<BulletView>(64);

        private readonly Dictionary<int, BulletView> _activeViews = new Dictionary<int, BulletView>(64);

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        public void Spawn(int bulletId, Vector2 position, float diameter, Color color)
        {
            BulletView view;

            if (_freeViews.Count > 0)
            {
                view = _freeViews.Pop();
            }
            else
            {
                view = CreateView();
            }

            view.Show(position, diameter, color);
            _activeViews.Add(bulletId, view);
        }

        public void Move(int bulletId, Vector2 position)
        {
            BulletView view;

            if (_activeViews.TryGetValue(bulletId, out view))
            {
                view.transform.position = new Vector3(position.x, position.y, 0f);
            }
        }

        public void Despawn(int bulletId)
        {
            BulletView view;

            if (_activeViews.TryGetValue(bulletId, out view))
            {
                _activeViews.Remove(bulletId);
                view.Hide();
                _freeViews.Push(view);
            }
        }

        private BulletView CreateView()
        {
            GameObject viewObject = new GameObject("BulletView");
            viewObject.transform.SetParent(transform, false);
            viewObject.SetActive(false);

            BulletView view = viewObject.AddComponent<BulletView>();
            view.Initialize(_spriteFactory.GetCircleSprite());

            return view;
        }
    }
}
