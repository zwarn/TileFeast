using System;
using UnityEngine;

namespace Piece.model
{
    [CreateAssetMenu(fileName = "Aspect", menuName = "Aspect", order = 0)]
    public class AspectSO : ScriptableObject
    {
        public Sprite image;
        public String name;
    }
}