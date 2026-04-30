using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Placeables.BoardExpansions;
using Placeables.PersonalRulePlacements;
using Placeables.WallPlacements;
using Placeables.ZonePlacementS;
using Rules.EmotionRules;
using UnityEngine;
using Zones;

namespace Roguelike.Generators
{
    [CreateAssetMenu(fileName = "PlaceableOfferGenerator", menuName = "Roguelike/Generators/Placeable Generator")]
    public class PlaceableOfferGeneratorSO : ScriptableObject
    {
        [Header("Board Expansion / Wall Placement")]
        public BoardExpansionPreviewSettings boardExpansionSettings;
        public bool includeBoardExpansions = true;
        public bool includeWallPlacements = true;

        [Header("Zone Placement")]
        public List<ZoneSO> availableZones;
        public bool includeZonePlacements = true;

        [Header("Personal Rule Placement")]
        [SerializeReference] public List<EmotionRule> personalRuleTemplates;
        public Sprite personalRuleIcon;
        public bool includePersonalRules = true;

        public List<RoguelikeDraftOffer> GenerateGroup(int count, GameController gc)
        {
            var generators = BuildGeneratorList(gc);
            if (generators.Count == 0)
            {
                Debug.LogWarning("[PlaceableOfferGeneratorSO] No enabled sub-type generators.");
                return new List<RoguelikeDraftOffer>();
            }

            return Enumerable.Range(0, count)
                .Select(_ => generators[UnityEngine.Random.Range(0, generators.Count)]())
                .ToList();
        }

        private List<Func<RoguelikeDraftOffer>> BuildGeneratorList(GameController gc)
        {
            var list = new List<Func<RoguelikeDraftOffer>>();

            if (includeBoardExpansions && boardExpansionSettings != null)
                list.Add(() => CreateBoardExpansionOffer(gc));

            if (includeWallPlacements && boardExpansionSettings != null)
                list.Add(() => CreateWallPlacementOffer(gc));

            if (includeZonePlacements && availableZones is { Count: > 0 })
                list.Add(() => CreateZonePlacementOffer(gc));

            if (includePersonalRules && personalRuleTemplates is { Count: > 0 })
                list.Add(() => CreatePersonalRuleOffer(gc));

            return list;
        }

        private RoguelikeDraftOffer CreateBoardExpansionOffer(GameController gc)
        {
            var generator = new BoardExpansionPreviewGenerator(boardExpansionSettings);
            var placeable = RandomBoardExpansionFactory.Create(generator, gc);
            return new RoguelikeDraftOffer
            {
                DisplayName = "Board Expansion",
                PreviewSprite = placeable.PreviewSprite,
                Type = DraftOfferType.Placeable,
                Placeable = placeable,
            };
        }

        private RoguelikeDraftOffer CreateWallPlacementOffer(GameController gc)
        {
            var generator = new WallPlacementPreviewGenerator(boardExpansionSettings);
            var placeable = RandomWallPlacementFactory.Create(generator, gc);
            return new RoguelikeDraftOffer
            {
                DisplayName = "Wall Placement",
                PreviewSprite = placeable.PreviewSprite,
                Type = DraftOfferType.Placeable,
                Placeable = placeable,
            };
        }

        private RoguelikeDraftOffer CreateZonePlacementOffer(GameController gc)
        {
            var zoneType = availableZones[UnityEngine.Random.Range(0, availableZones.Count)];
            var generator = new ZonePlacementPreviewGenerator();
            var placeable = RandomZonePlacementFactory.Create(generator, gc, zoneType);
            return new RoguelikeDraftOffer
            {
                DisplayName = $"Zone: {zoneType.name}",
                PreviewSprite = placeable.PreviewSprite,
                Type = DraftOfferType.Placeable,
                Placeable = placeable,
            };
        }

        private RoguelikeDraftOffer CreatePersonalRuleOffer(GameController gc)
        {
            var rule = personalRuleTemplates[UnityEngine.Random.Range(0, personalRuleTemplates.Count)];
            var placeable = new PersonalRulePlacement(rule, personalRuleIcon, gc);
            return new RoguelikeDraftOffer
            {
                DisplayName = rule.GetDescription(),
                PreviewSprite = personalRuleIcon,
                Type = DraftOfferType.Placeable,
                Placeable = placeable,
            };
        }
    }
}