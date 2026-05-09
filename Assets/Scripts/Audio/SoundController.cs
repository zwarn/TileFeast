using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private SoundLibrarySO _library;

        private AudioSource _source;
        private readonly Dictionary<SoundEventSO, float> _lastPlayTimes = new();

        private void Awake() => _source = GetComponent<AudioSource>();

        public void Play(GameSoundEvent evt)
        {
            var so = _library?.GetSound(evt);
            if (so == null) return;
            if (!CanPlay(so)) return;
            so.Play(_source);
            _lastPlayTimes[so] = Time.time;
        }

        public void PlayDirect(SoundEventSO so)
        {
            if (so == null || !CanPlay(so)) return;
            so.Play(_source);
            _lastPlayTimes[so] = Time.time;
        }

        private bool CanPlay(SoundEventSO so)
        {
            if (so.cooldown <= 0f) return true;
            return !_lastPlayTimes.TryGetValue(so, out float last) || Time.time - last >= so.cooldown;
        }
    }
}
