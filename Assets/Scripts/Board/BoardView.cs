using System;
using System.Collections.Generic;
using Hand;
using Piece.model;
using Piece.view;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour, IPointerClickHandler
    {
        [Inject] private DiContainer _container;
        [Inject] private BoardController _boardController;
        [Inject] private InteractionController _interactionController;

        [SerializeField] private PieceView pieceViewPrefab;
        [SerializeField] private Transform pieceViewParent;

        private Dictionary<PlacedPiece, PieceView> _views = new();

        private void OnEnable()
        {
            _boardController.OnPiecePlaced += PiecePlaced;
            _boardController.OnPieceRemoved += PieceRemoved;
        }

        private void OnDisable()
        {
            _boardController.OnPiecePlaced -= PiecePlaced;
            _boardController.OnPieceRemoved -= PieceRemoved;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = Vector2Int.RoundToInt(worldClickPoint);
            
            _interactionController.BoardClicked(position);
        }
    }
}