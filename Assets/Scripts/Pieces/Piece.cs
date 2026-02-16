using System;
using System.Collections.Generic;
using System.Linq;
using Pieces.Aspects;
using UnityEngine;

namespace Pieces
{
    public class Piece
    {
        public List<Vector2Int> shape;
        public Sprite sprite;
        public List<Aspect> aspects;

        private bool _locked;
        public bool locked
        {
            get => _locked;
            set
            {
                if (_locked == value) return;
                _locked = value;
                OnChanged?.Invoke();
            }
        }

        public event Action OnChanged;

        public Piece(List<Vector2Int> shape, Sprite sprite, List<Aspect> aspects, bool locked)
        {
            this.shape = shape;
            this.sprite = sprite;
            this.aspects = aspects;
            _locked = locked;
        }

        public Piece(PieceSO pieceSO, bool locked) : this(pieceSO.shape, pieceSO.sprite,
            pieceSO.aspects.Select(so => new Aspect(so)).ToList(), locked)
        {
        }
    }
}