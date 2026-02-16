using UnityEngine;

namespace Pieces.Aspects
{
    public class AspectView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer renderer;

        public void SetData(Aspect aspect)
        {
            renderer.sprite = aspect.icon;
        }
    }
}