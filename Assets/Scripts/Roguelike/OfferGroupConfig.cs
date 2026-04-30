using System;
using UnityEngine;

namespace Roguelike
{
    public enum OfferGroupType { Pieces, Placeables, Rules }

    [Serializable]
    public class OfferGroupConfig
    {
        public OfferGroupType type;
        [Tooltip("Number of draft groups of this type to present this turn")]
        public int groupCount = 1;
    }
}
