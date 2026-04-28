using Pieces.Animation;
using Pieces.Aspects;
using Rules;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Pieces
{
    public class PieceView : MonoBehaviour, IPlaceableView
    {
        private PlaceablePiece _placeable;

        public void Bind(IPlaceable placeable) => _placeable = (PlaceablePiece)placeable;
        public void Activate()   => SetData(_placeable.Piece);
        public void Deactivate() => SetData(null);
        public void UpdateFrame(Grid grid, Vector3 mouseWorldPos, Vector2Int boardCell) { }
        public void OnRotated() { }

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AspectListView aspectListView;
        [SerializeField] private FaceView faceView;

        [Inject] private RulesController _rulesController;

        private PieceWithRotation _piece;
        private PieceEmotion? _lastEmotion;

        private void OnEnable()
        {
            _rulesController.OnEvaluationChanged += OnEvaluationChanged;
        }

        private void OnDisable()
        {
            _rulesController.OnEvaluationChanged -= OnEvaluationChanged;
        }

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _piece?.Rotation ?? 0);
        }

        private void OnDestroy()
        {
            if (_piece != null)
            {
                _piece.Piece.OnChanged -= OnPieceChanged;
            }
        }

        public void SetData(PieceWithRotation piece)
        {
            if (_piece != null)
                _piece.Piece.OnChanged -= OnPieceChanged;

            _piece = piece;
            _lastEmotion = null;
            gameObject.SetActive(_piece != null);

            if (_piece != null)
            {
                _piece.Piece.OnChanged += OnPieceChanged;
                spriteRenderer.sprite = piece.Piece.sprite;
                transform.rotation = Quaternion.Euler(0, 0, 90 * _piece.Rotation);

                aspectListView.SetData(piece.Piece);
                faceView?.SetData(piece.Piece);
            }
        }

        private void OnPieceChanged()
        {
            if (_piece != null)
                aspectListView.SetData(_piece.Piece);
        }

        private void OnEvaluationChanged(EmotionEvaluationResult result)
        {
            if (_piece == null) return;
            if (!_piece.Piece.hasEmotions) return;

            PieceEmotion emotion = PieceEmotion.Neutral;
            PieceEmotionState matchedState = null;
            foreach (var state in result.PieceStates)
            {
                if (state.Piece.Piece == _piece.Piece)
                {
                    emotion = state.FinalEmotion;
                    matchedState = state;
                    break;
                }
            }

            if (matchedState != null)
                aspectListView.SetData(_piece.Piece, matchedState.Piece.AllAspects);

            if (faceView == null || emotion == _lastEmotion) return;
            _lastEmotion = emotion;
            faceView.SetEmotion(emotion);
        }
    }
}