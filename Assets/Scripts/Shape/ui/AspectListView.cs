using System.Collections.Generic;
using System.Linq;
using Shape.model;
using UnityEngine;

namespace Shape.ui
{
    public class AspectListView : MonoBehaviour
    {
        [SerializeField] private AspectView prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private SortingLayer targetSorting;

        private readonly List<AspectView> _aspectViews = new();

        public void SetData(List<AspectSO> aspects, ShapeSO shape)
        {
            int viewsNeeded = aspects.Count - _aspectViews.Count;
            for (int i = 0; i < viewsNeeded; i++)
            {
                var aspectView = Instantiate(prefab, parent);
                _aspectViews.Add(aspectView);
            }

            for (int i = 0; i < _aspectViews.Count; i++)
            {
                if (i >= aspects.Count)
                {
                    _aspectViews[i].gameObject.SetActive(false);
                    return;
                }

                _aspectViews[i].SetData(aspects[i], targetSorting);
                _aspectViews[i].gameObject.SetActive(true);
                SetPosition(i, _aspectViews[i], shape);
            }
        }

        private void SetPosition(int index, AspectView aspectView, ShapeSO shape)
        {
            var position = shape.tilePosition.OrderBy(pos => pos.x).ThenByDescending(pos => pos.y).First();

            Vector2 delta = index switch
            {
                0 => new Vector2(-0.25f, 0.25f),
                1 => new Vector2(0.25f, 0.25f),
                2 => new Vector2(-0.25f, -0.25f),
                _ => new Vector2(-0.25f, 0.25f)
            };

            aspectView.gameObject.transform.localPosition = new Vector3(position.x + delta.x, position.y + delta.y, 0);
        }
    }
}