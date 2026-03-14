using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    public class FaceView : MonoBehaviour
    {
        [SerializeField] private GameObject leftEye;
        [SerializeField] private GameObject rightEye;
        [SerializeField] private GameObject mouthSingle;
        [SerializeField] private GameObject mouthDouble;

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
            PlaceEyes(piece.shape);
            PlaceMouth(piece.shape);
            StartBlink();
        }

        private void OnDisable() => StopBlink();

        private void StartBlink()
        {
            var leftEyeAnim = leftEye ? leftEye.GetComponent<Animator>() : null;
            var rightEyeAnim = rightEye ? rightEye.GetComponent<Animator>() : null;

            if (leftEyeAnim != null || rightEyeAnim != null)
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
            var leftEyeAnim = leftEye ? leftEye.GetComponent<Animator>() : null;
            var rightEyeAnim = rightEye ? rightEye.GetComponent<Animator>() : null;

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(blinkIntervalMin, blinkIntervalMax));
                leftEyeAnim?.SetTrigger("Blink");
                rightEyeAnim?.SetTrigger("Blink");
            }
        }

        // -----------------------------------------------------------------

        private void PlaceEyes(List<Vector2Int> shape)
        {
            int topY = shape.Max(p => p.y);
            var topTiles = shape.Where(p => p.y == topY).ToList();

            int leftX = topTiles.Min(p => p.x);
            int rightX = topTiles.Max(p => p.x);

            leftEye.transform.localPosition = new Vector3(leftX, topY, 0);
            rightEye.transform.localPosition = new Vector3(rightX, topY, 0);
        }

        private void PlaceMouth(List<Vector2Int> shape)
        {
            // Rows sorted ascending so we scan from the visual bottom upward.
            var rows = shape.Select(p => p.y).Distinct().OrderBy(y => y).ToList();

            foreach (int row in rows)
            {
                var xs = shape.Where(p => p.y == row).Select(p => p.x).OrderBy(x => x).ToList();
                int minX = xs.First();
                int maxX = xs.Last();
                int count = xs.Count;

                // Contiguous check: no horizontal gaps in this row.
                if (maxX - minX + 1 != count)
                    continue;

                // Centre of the row's span in tile coordinates.
                float centerX = (minX + maxX) / 2f;
                bool isEven = count % 2 == 0;

                if (isEven)
                {
                    mouthSingle.SetActive(false);
                    mouthDouble.SetActive(true);
                    // centerX lands on a half-integer → centre of the 2-tile sprite.
                    mouthDouble.transform.localPosition = new Vector3(centerX, row, 0);
                }
                else
                {
                    mouthDouble.SetActive(false);
                    mouthSingle.SetActive(true);
                    // centerX lands on an integer → centre of the 1-tile sprite.
                    mouthSingle.transform.localPosition = new Vector3(Mathf.RoundToInt(centerX), row, 0);
                }

                return;
            }

            // No contiguous row found — hide mouth.
            mouthSingle.SetActive(false);
            mouthDouble.SetActive(false);
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