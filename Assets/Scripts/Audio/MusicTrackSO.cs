using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "MusicTrack", menuName = "Audio/Music Track")]
    public class MusicTrackSO : ScriptableObject
    {
        public AudioClip clip;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 1f;
    }
}
