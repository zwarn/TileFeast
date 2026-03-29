using System;
using System.Collections.Generic;
using System.Linq;
using Pieces.Aspects;
using UnityEngine;

namespace Pieces
{
    public class Piece
    {
        public PieceSO sourceSO;
        public List<Vector2Int> shape;
        public Sprite sprite;
        public Sprite previewSprite;
        public List<Aspect> aspects;

        public Vector2Int leftEyePosition;
        public Vector2Int rightEyePosition;
        public bool       hasMouth;
        public Vector2    mouthPosition;
        public bool       mouthDouble;
        public bool       hasEmotions;

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

        public Piece(PieceSO sourceSO, List<Vector2Int> shape, Sprite sprite, Sprite previewSprite, List<Aspect> aspects, bool locked)
        {
            this.sourceSO = sourceSO;
            this.shape = shape;
            this.sprite = sprite;
            this.previewSprite = previewSprite;
            this.aspects = aspects;
            _locked = locked;
        }

        public Piece(PieceSO pieceSO, bool locked) : this(pieceSO, pieceSO.shape, pieceSO.sprite, pieceSO.previewSprite,
            pieceSO.aspects.Select(so => new Aspect(so)).ToList(), locked)
        {
            leftEyePosition  = pieceSO.leftEyePosition;
            rightEyePosition = pieceSO.rightEyePosition;
            hasMouth         = pieceSO.hasMouth;
            mouthPosition    = pieceSO.mouthPosition;
            mouthDouble      = pieceSO.mouthDouble;
            hasEmotions      = pieceSO.hasEmotions;
        }
    }
}