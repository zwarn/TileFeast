using UnityEngine;

namespace Piece.aspect
{
    [CreateAssetMenu(fileName = "Aspect", menuName = "Aspect", order = 0)]
    public class AspectSO : ScriptableObject
    {
        public Sprite image;
        public string name;
    }
}