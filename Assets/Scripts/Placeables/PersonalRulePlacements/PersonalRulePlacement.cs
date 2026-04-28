using Core;
using Rules.EmotionRules;
using Tools;
using UnityEngine;

namespace Placeables.PersonalRulePlacements
{
    public class PersonalRulePlacement : IPlaceable
    {
        private readonly EmotionRule _rule;
        private readonly GameController _gameController;

        public PersonalRulePlacement(EmotionRule rule, Sprite previewSprite, GameController gameController)
        {
            _rule = rule;
            PreviewSprite = previewSprite;
            _gameController = gameController;
        }

        public Sprite PreviewSprite { get; }

        public bool IsValidPlacement(Vector2Int boardCell) => _gameController.HasPieceAt(boardCell);

        public bool TryPlace(Vector2Int boardCell) => _gameController.TryAddPersonalRule(boardCell, _rule);

        public void Rotate(int direction) { }

        public void OnDiscard() => _gameController.ReturnToSupply(this);
    }
}
