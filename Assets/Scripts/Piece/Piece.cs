using System.Collections.Generic;
using System.Linq;
using Piece.aspect;
using UnityEngine;

namespace Piece
{
    public class Piece
    {
        public ShapeSO shape;
        public Sprite sprite;
        public List<Aspect> aspects;
        public bool locked;

        public Piece(ShapeSO shape, Sprite sprite, List<Aspect> aspects, bool locked)
        {
            this.shape = shape;
            this.sprite = sprite;
            this.aspects = aspects;
            this.locked = locked;
        }

        public Piece(PieceSO pieceSO, bool locked) : this(pieceSO.shape, pieceSO.sprite,
            pieceSO.aspects.Select(so => new Aspect(so)).ToList(), locked)
        {
        }
    }
}