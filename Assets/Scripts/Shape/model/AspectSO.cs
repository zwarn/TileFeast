using System;
using UnityEngine;

namespace Shape.model
{
    [CreateAssetMenu(fileName = "Aspect", menuName = "Aspect", order = 0)]
    public class AspectSO : ScriptableObject
    {
        public Sprite image;
        public String name;
    }
}