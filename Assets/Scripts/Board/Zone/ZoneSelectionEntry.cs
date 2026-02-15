using Hand.Tool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Board.Zone
{
    public class ZoneSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;

        [SerializeField] private GameObject highlights;
        [SerializeField] private GameObject lowlights;

        [Inject] private ToolController _toolController;
        [Inject] private ZoneTool _zoneTool;

        private ZoneSO _zoneType;

        private void Update()
        {
            if (_toolController.CurrentToolType == ToolType.ZonesTool)
            {
                UpdateHighlight(_zoneTool.SelectedZoneType == _zoneType);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _toolController.SelectZone(_zoneType);
        }

        public void SetData(ZoneSO zoneType)
        {
            _zoneType = zoneType;
            image.sprite = zoneType.zoneImage;

            highlights.SetActive(false);
        }

        private void UpdateHighlight(bool highlight)
        {
            highlights.SetActive(highlight);
            lowlights.SetActive(!highlight);
        }
    }
}