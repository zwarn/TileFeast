using System.Collections.Generic;
using System.Linq;
using Core;
using Piece;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Board
{
    public class PiecesOnBoardView : MonoBehaviour
    {
        [SerializeField] private PieceView pieceViewPrefab;
        [SerializeField] private Transform pieceViewParent;

        private readonly Dictionary<PlacedPiece, PieceView> _views = new();
        [Inject] private BoardController _boardController;
        [Inject] private DiContainer _container;

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
            pieceView.transform.localPosition = new Vector3(piece.Position.x, piece.Position.y);
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