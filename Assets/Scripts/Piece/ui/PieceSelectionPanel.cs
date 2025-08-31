using System.Collections.Generic;
using Piece.controller;
using Piece.model;
using UnityEngine;
using Zenject;

namespace Piece.ui
{
    public class PieceSelectionPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private PieceSupplyController _pieceSupply;


        [SerializeField] private PieceSelectionEntry prefab;
        [SerializeField] private Transform entryParent;

        private readonly Dictionary<PieceSO, PieceSelectionEntry> _entries = new();

        private void Start()
        {
            _pieceSupply.GetPieces().ForEach(AddEntry);
        }
        
        private void OnEnable()
        {
            _pieceSupply.OnPieceAdded += PieceAdded;
            _pieceSupply.OnPieceRemoved += PieceRemoved;
        }

        private void OnDisable()
        {
            _pieceSupply.OnPieceAdded -= PieceAdded;
            _pieceSupply.OnPieceRemoved -= PieceRemoved;
        }

        private void PieceRemoved(PieceSO piece)
        {
            var entry = _entries[piece];
            Destroy(entry.gameObject);
            _entries.Remove(piece);
        }

        private void PieceAdded(PieceSO piece)
        {
            AddEntry(piece);
        }
        
        
        private void AddEntry(PieceSO piece)
        {
            var entryObject = _container.InstantiatePrefab(prefab, entryParent);
            var entry = entryObject.GetComponent<PieceSelectionEntry>();
            entry.SetData(piece);
            _entries.Add(piece, entry);
        }
    }
}