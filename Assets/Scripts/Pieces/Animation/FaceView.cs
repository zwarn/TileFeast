using System.Collections;
using UnityEngine;

namespace Pieces.Animation
{
    public class FaceView : MonoBehaviour
    {
        private static readonly int Blink = Animator.StringToHash("Blink");

        [SerializeField] private GameObject leftEye;
        [SerializeField] private GameObject rightEye;
        [SerializeField] private GameObject mouthSingle;
        [SerializeField] private GameObject mouthDouble;
        [SerializeField] private Animator animator;

        [SerializeField] private float blinkIntervalMin = 4f;
        [SerializeField] private float blinkIntervalMax = 10f;

        private Coroutine _blinkRoutine;

        public void SetData(Piece piece)
        {
            StopBlink();

            if (piece == null)
            {
                SetAllActive(false);
                return;
            }

            SetAllActive(true);

            leftEye.transform.localPosition = new Vector3(piece.leftEyePosition.x, piece.leftEyePosition.y, 0);
            rightEye.transform.localPosition = new Vector3(piece.rightEyePosition.x, piece.rightEyePosition.y, 0);

            mouthSingle.SetActive(!piece.mouthDouble && piece.hasMouth);
            mouthDouble.SetActive(piece.mouthDouble && piece.hasMouth);
            if (piece.hasMouth)
                (piece.mouthDouble ? mouthDouble : mouthSingle)
                    .transform.localPosition = new Vector3(piece.mouthPosition.x, piece.mouthPosition.y, 0);

            StartBlink();
        }

        private void OnDisable() => StopBlink();

        private void StartBlink()
        {
            if (animator != null)
                _blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        private void StopBlink()
        {
            if (_blinkRoutine != null)
                StopCoroutine(_blinkRoutine);
            _blinkRoutine = null;
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(blinkIntervalMin, blinkIntervalMax));
                animator?.SetTrigger(Blink);
            }
        }

        // -----------------------------------------------------------------

        private void SetAllActive(bool active)
        {
            if (leftEye) leftEye.SetActive(active);
            if (rightEye) rightEye.SetActive(active);
            if (mouthSingle) mouthSingle.SetActive(active);
            if (mouthDouble) mouthDouble.SetActive(active);
        }
    }
}