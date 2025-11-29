using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Piece
{
    public class Piece
    {
        public List<Vector2Int> shape;
        public Sprite sprite;
        public List<Aspect.Aspect> aspects;
        public bool locked;

        public Piece(List<Vector2Int> shape, Sprite sprite, List<Aspect.Aspect> aspects, bool locked)
        {
            this.shape = shape;
            this.sprite = sprite;
            this.aspects = aspects;
            this.locked = locked;
        }

        public Piece(PieceSO pieceSO, bool locked) : this(pieceSO.shape, pieceSO.sprite,
            pieceSO.aspects.Select(so => new Aspect.Aspect(so)).ToList(), locked)
        {
        }
    }
}