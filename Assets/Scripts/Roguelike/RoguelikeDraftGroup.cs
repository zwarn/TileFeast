using System.Collections.Generic;

namespace Roguelike
{
    public class RoguelikeDraftGroup
    {
        public readonly List<RoguelikeOfferSO> Options;
        public bool IsResolved { get; private set; }
        public int ChosenIndex { get; private set; } = -1;

        public RoguelikeDraftGroup(List<RoguelikeOfferSO> options)
        {
            Options = options;
        }

        public void Resolve(int chosenIndex)
        {
            ChosenIndex = chosenIndex;
            IsResolved = true;
        }
    }
}
