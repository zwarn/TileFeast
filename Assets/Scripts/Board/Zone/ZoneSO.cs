using Rules.ZoneRules;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Board.Zone
{
    [CreateAssetMenu(fileName = "Zone", menuName = "Zone", order = 0)]
    public class ZoneSO : ScriptableObject
    {
        public TileBase zoneTile;
        public Sprite zoneImage;
    }
}