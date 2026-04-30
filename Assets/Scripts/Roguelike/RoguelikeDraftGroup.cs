using System.Collections.Generic;

namespace Roguelike
{
    public class RoguelikeDraftGroup
    {
        public readonly List<RoguelikeDraftOffer> Options;
        public readonly OfferGroupType Type;
        public bool IsResolved { get; private set; }
        public int ChosenIndex { get; private set; } = -1;

        public RoguelikeDraftGroup(List<RoguelikeDraftOffer> options, OfferGroupType type)
        {
            Options = options;
            Type = type;
        }

        public void Resolve(int chosenIndex)
        {
            ChosenIndex = chosenIndex;
            IsResolved = true;
        }
    }
}
