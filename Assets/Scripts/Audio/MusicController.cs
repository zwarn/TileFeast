using System.Collections;
using UnityEngine;

namespace Audio
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private AudioSource _sourceA;
        [SerializeField] private AudioSource _sourceB;
        [SerializeField] private MusicLibrarySO _library;

        private AudioSource _active;
        private AudioSource _standby;
        private MusicContext _currentContext = MusicContext.None;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _active = _sourceA;
            _standby = _sourceB;
        }

        public void SetContext(MusicContext ctx)
        {
            if (ctx == _currentContext) return;
            var track = _library?.GetTrack(ctx);
            if (track == null && ctx != MusicContext.None) return;
            _currentContext = ctx;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeTo(track));
        }

        private IEnumerator CrossfadeTo(MusicTrackSO track)
        {
            float dur = _library?.crossfadeDuration ?? 1f;
            float startVol = _active.volume;
            float targetVol = track?.volume ?? 1f;

            _standby.clip = track?.clip;
            _standby.loop = track?.loop ?? false;
            _standby.volume = 0f;
            if (track != null) _standby.Play();

            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float pct = t / dur;
                _active.volume = Mathf.Lerp(startVol, 0f, pct);
                _standby.volume = Mathf.Lerp(0f, targetVol, pct);
                yield return null;
            }

            _active.Stop();
            _active.volume = 0f;
            _standby.volume = targetVol;
            (_active, _standby) = (_standby, _active);
            _fadeRoutine = null;
        }
    }
}
