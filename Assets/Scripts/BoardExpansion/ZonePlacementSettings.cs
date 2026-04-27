using UnityEngine;
using Zones;

namespace BoardExpansion
{
    [CreateAssetMenu(fileName = "ZonePlacementSettings", menuName = "BoardExpansion/Zone Placement Settings")]
    public class ZonePlacementSettings : ScriptableObject
    {
        public ZoneSO zoneType;
    }
}
