using System;
using Tools;
using UnityEngine;

namespace Roguelike
{
    // Wraps any IPlaceable acquired from a draft offer.
    // Intercepts OnDiscard so the item does not return to supply —
    // instead the draft group stays pending and the panel re-appears.
    public class DraftPlaceable : IPlaceable
    {
        private readonly IPlaceable _inner;
        private readonly Action _onPlaced;
        private readonly Action _onDiscarded;

        public DraftPlaceable(IPlaceable inner, Action onPlaced, Action onDiscarded)
        {
            _inner = inner;
            _onPlaced = onPlaced;
            _onDiscarded = onDiscarded;
        }

        public IPlaceable Inner => _inner;
        public Sprite PreviewSprite => _inner.PreviewSprite;

        public bool TryPlace(Vector2Int boardCell)
        {
            var success = _inner.TryPlace(boardCell);
            if (success) _onPlaced?.Invoke();
            return success;
        }

        public void Rotate(int direction) => _inner.Rotate(direction);

        public void OnDiscard() => _onDiscarded?.Invoke();
    }
}
