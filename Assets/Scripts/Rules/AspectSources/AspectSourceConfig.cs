using System;

namespace Rules.AspectSources
{
    [Serializable]
    public class AspectSourceConfig
    {
        public AspectSourceSO source;
        [UnityEngine.SerializeReference] public AspectSourceArgs args;
    }
}
