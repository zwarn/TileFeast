using UnityEngine;
using Zones;

namespace Placeables.ZonePlacementS
{
    [CreateAssetMenu(fileName = "ZonePlacementSettings", menuName = "BoardExpansion/Zone Placement Settings")]
    public class ZonePlacementSettings : ScriptableObject
    {
        public ZoneSO zoneType;
    }
}