using System;

namespace Rules.Components
{
    [Serializable]
    public class IntRange
    {
        [UnityEngine.Tooltip("Minimum value (inclusive)")]
        public int min = 0;

        [UnityEngine.Tooltip("Maximum value (inclusive). -1 means unlimited")]
        public int max = -1;

        public bool Contains(int value) => value >= min && (max < 0 || value <= max);

        public string GetDescription()
        {
            if (max < 0)
            {
                if (min <= 0) return "any";
                return $"{min}+";
            }
            if (min <= 0) return $"at most {max}";
            if (min == max) return $"exactly {min}";
            return $"{min}-{max}";
        }
    }
}
