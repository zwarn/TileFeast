using System;
using System.Collections.Generic;
using Piece.model;
using Scenario;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Piece.controller
{
    public class PieceSupplyController : MonoBehaviour
    {
        [SerializeField] private List<PieceSO> pieces;

        public event Action<PieceSO> OnPieceAdded;
        public event Action<PieceSO> OnPieceRemoved;
        public event Action<List<PieceSO>> OnPiecesReplaced;

        [Inject] private ScenarioController _scenarioController;

        private void OnEnable()
        {
            _scenarioController.OnScenarioChanged += ChangeAvailablePieces;
        }

        private void OnDisable()
        {
            _scenarioController.OnScenarioChanged -= ChangeAvailablePieces;
        }

        public List<PieceSO> GetPieces()
        {
            return pieces;
        }

        public void RemovePiece(PieceSO piece)
        {
            pieces.Remove(piece);
            RemovePieceEvent(piece);
        }

        public void AddPiece(PieceWithRotation piece)
        {
            pieces.Add(piece.Piece);
            AddPieceEvent(piece.Piece);
        }

        private void ChangeAvailablePieces(ScenarioSO scenario)
        {
            pieces.Clear();
            pieces.AddRange(scenario.availablePieces);
            ReplacePiecesEvent(pieces);
        }

        public void RemovePieceEvent(PieceSO piece)
        {
            OnPieceRemoved?.Invoke(piece);
        }

        public void AddPieceEvent(PieceSO piece)
        {
            OnPieceAdded?.Invoke(piece);
        }

        public void ReplacePiecesEvent(List<PieceSO> newPieces)
        {
            OnPiecesReplaced?.Invoke(newPieces);
        }
    }
}
