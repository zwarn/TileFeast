using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "SoundEvent", menuName = "Audio/Sound Event")]
    public class SoundEventSO : ScriptableObject
    {
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitchMin = 0.95f;
        public float pitchMax = 1.05f;
        public float cooldown = 0f;

        public void Play(AudioSource source)
        {
            if (clips == null || clips.Length == 0) return;
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
