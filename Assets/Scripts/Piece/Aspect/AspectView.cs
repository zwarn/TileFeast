using UnityEngine;

namespace Piece.aspect
{
    public class AspectView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer renderer;

        public void SetData(AspectSO aspect)
        {
            renderer.sprite = aspect.image;
        }
    }
}