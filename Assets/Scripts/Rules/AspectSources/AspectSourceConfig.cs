using System;

namespace Rules.AspectSources
{
    [Serializable]
    public class AspectSourceConfig
    {
        [UnityEngine.SerializeReference] public AspectSource source;
    }
}
