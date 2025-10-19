using System;
using System.Collections.Generic;
using System.Linq;
using Piece;
using UnityEngine;

namespace Scenario
{
    [Serializable]
    public class LockedPieceList
    {
        [SerializeField] private List<LockedPieceData> data;

        public List<PlacedPiece> LockedPieces()
        {
            return data.SelectMany(d => d.instances.Select(instance =>
                new PlacedPiece(new Piece.Piece(d.type.piece, true), instance.rotation, instance.position))).ToList();
        }
    }

    [Serializable]
    public class LockedPieceData
    {
        public LockedPieceType type;
        public List<LockedPieceInstance> instances;
    }

    [Serializable]
    public class LockedPieceType
    {
        public PieceSO piece;
    }

    [Serializable]
    public class LockedPieceInstance
    {
        public Vector2Int position;
        public int rotation;
    }
}