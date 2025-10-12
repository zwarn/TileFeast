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

        public Piece(ShapeSO shape, Sprite sprite, List<Aspect> aspects)
        {
            this.shape = shape;
            this.sprite = sprite;
            this.aspects = aspects;
        }

        public Piece(PieceSO pieceSO) : this(pieceSO.shape, pieceSO.sprite,
            pieceSO.aspects.Select(so => new Aspect(so)).ToList())
        {
        }
    }
}