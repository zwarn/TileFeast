using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "MusicLibrary", menuName = "Audio/Music Library")]
    public class MusicLibrarySO : ScriptableObject
    {
        [Serializable]
        public class MusicBinding
        {
            public MusicContext context;
            public MusicTrackSO track;
        }

        public List<MusicBinding> bindings = new();
        public float crossfadeDuration = 1f;

        public MusicTrackSO GetTrack(MusicContext ctx)
        {
            foreach (var b in bindings)
                if (b.context == ctx) return b.track;
            return null;
        }
    }
}
