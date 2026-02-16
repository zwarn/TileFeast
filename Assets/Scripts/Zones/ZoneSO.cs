using Zones.Rules;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zones
{
    [CreateAssetMenu(fileName = "Zone", menuName = "Zone", order = 0)]
    public class ZoneSO : ScriptableObject
    {
        public TileBase zoneTile;
        public Sprite zoneImage;
        [SerializeReference] public ZoneRule zoneRule;
    }
}