using UnityEngine;

namespace Pieces.Aspects
{
    public class Aspect
    {
        public Sprite icon;
        public string name;

        public Aspect(Sprite icon, string name)
        {
            this.icon = icon;
            this.name = name;
        }

        public Aspect(AspectSO aspectSO) : this(aspectSO.image, aspectSO.name)
        {
            
        }

        protected bool Equals(Aspect other)
        {
            return name == other.name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Aspect)obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}