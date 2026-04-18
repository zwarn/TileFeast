using System;
using Pieces;
using Pieces.Aspects;
using Rules.Filters;

namespace Rules.AspectSources
{
    /// <summary>
    /// Grants a dynamic aspect to any piece matching the configured <see cref="PieceFilter"/>.
    /// Re-uses the same filter library as emotion rules, so zone-based, aspect-based, or
    /// composite filters all compose here.
    /// </summary>
    [Serializable]
    public class GrantAspectSource : AspectSource
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Pieces matching this filter receive the granted aspect")]
        public PieceFilter filter = new AllPiecesFilter();

        [UnityEngine.Tooltip("Aspect to add to matching pieces")]
        public AspectSO grantedAspect;

        public override void Apply(PlacedPiece piece, EmotionContext context)
        {
            if (grantedAspect == null) return;
            if (filter == null || !filter.Matches(piece, context)) return;
            piece.DynamicAspects.Add(new Aspect(grantedAspect));
        }

        public override string GetDescription()
        {
            var filterText = filter != null ? filter.GetDescription() : "(no filter)";
            var aspectName = grantedAspect != null ? grantedAspect.name : "?";
            return $"{filterText} gain the {aspectName} aspect";
        }
    }
}
