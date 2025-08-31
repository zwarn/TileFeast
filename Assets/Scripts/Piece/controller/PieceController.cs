using Piece.model;
using Piece.view;
using UnityEngine;
using UnityEngine.Serialization;

namespace Piece.controller
{
    public class PieceController : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;
        [SerializeField] private PieceSO piece;

        private PieceWithRotation _currentPiece;

        private void Start()
        {
            _currentPiece = new PieceWithRotation(piece, 0);
            pieceView.SetData(_currentPiece);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                _currentPiece.Rotate(1);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                _currentPiece.Rotate(-1);
            }
        }
    }
}