using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;

namespace Rules.EmotionRules
{
    [CreateAssetMenu(menuName = "EmotionRule/AspectAdjacency", fileName = "AspectAdjacencyEmotionRule")]
    public class AspectAdjacencyEmotionRuleSO : EmotionRuleSO
    {
        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context, EmotionRuleArgs args)
        {
            var a = (AspectAdjacencyArgs)args;

            if (a.applyToAspect != null && !piece.Piece.aspects.Contains(new Aspect(a.applyToAspect)))
                return null;

            var neighbors = RulesHelper.GetNeighborPieces(piece, context.TileArray);
            int count = neighbors.Count(n => n.aspects.Contains(new Aspect(a.neighborAspect)));

            bool conditionMet = count >= a.minNeighborCount &&
                                (a.maxNeighborCount < 0 || count <= a.maxNeighborCount);

            if (conditionMet)
            {
                return new EmotionEffect(a.emotionWhenMet,
                    $"Adjacent to {count} {a.neighborAspect.name} neighbor(s)", this);
            }

            if (a.emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(a.emotionWhenNotMet,
                $"Only {count} {a.neighborAspect.name} neighbor(s) (needs {a.minNeighborCount})", this);
        }

        public override string GetDescription(EmotionRuleArgs args)
        {
            var a = (AspectAdjacencyArgs)args;
            var target = a.applyToAspect != null ? $"{a.applyToAspect.name} pieces" : "Pieces";
            var range = a.maxNeighborCount >= 0
                ? $"{a.minNeighborCount}-{a.maxNeighborCount}"
                : $"{a.minNeighborCount}+";
            return $"{target} are {a.emotionWhenMet} when adjacent to {range} {a.neighborAspect.name} tile(s)";
        }
    }
}
