using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
    public class SoundLibrarySO : ScriptableObject
    {
        [Serializable]
        public class SoundBinding
        {
            public GameSoundEvent soundEvent;
            public SoundEventSO sound;
        }

        public List<SoundBinding> bindings = new();

        public SoundEventSO GetSound(GameSoundEvent evt)
        {
            foreach (var b in bindings)
                if (b.soundEvent == evt) return b.sound;
            return null;
        }
    }
}
