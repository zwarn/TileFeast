using System.Collections.Generic;
using System.Linq;
using Core;
using Piece;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private PieceView pieceViewPrefab;
        [SerializeField] private Transform pieceViewParent;

        private readonly Dictionary<PlacedPiece, PieceView> _views = new();
        [Inject] private BoardController _boardController;
        [Inject] private DiContainer _container;
        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _boardController.OnPiecePlaced += PiecePlaced;
            _boardController.OnPieceRemoved += PieceRemoved;
            _boardController.OnBoardReset += ResetPieces;
        }

        private void OnDisable()
        {
            _boardController.OnPiecePlaced -= PiecePlaced;
            _boardController.OnPieceRemoved -= PieceRemoved;
            _boardController.OnBoardReset -= ResetPieces;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = Vector2Int.RoundToInt(worldClickPoint);

            _gameController.BoardClicked(position);
        }

        private void ResetPieces(List<PlacedPiece> newPieces)
        {
            _views.Keys.ToList().ForEach(PieceRemoved);
            newPieces.ForEach(PiecePlaced);
        }

        private void PiecePlaced(PlacedPiece piece)
        {
            var viewObject = _container.InstantiatePrefab(pieceViewPrefab);
            viewObject.transform.parent = pieceViewParent;
            var pieceView = viewObject.GetComponent<PieceView>();
            pieceView.SetData(new PieceWithRotation(piece.Piece, piece.Rotation));
            pieceView.transform.position = new Vector3(piece.Position.x, piece.Position.y);
            _views.Add(piece, pieceView);
        }

        private void PieceRemoved(PlacedPiece piece)
        {
            var pieceView = _views[piece];
            _views.Remove(piece);
            Destroy(pieceView.gameObject);
        }
    }
}