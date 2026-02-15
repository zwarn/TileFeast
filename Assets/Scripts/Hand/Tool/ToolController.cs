using System;
using System.Collections.Generic;
using System.Linq;
using Board.Zone;
using UnityEngine;

namespace Hand.Tool
{
    public class ToolController : MonoBehaviour
    {
        [SerializeField] private List<ToolBase> allTools = new List<ToolBase>();

        private ToolBase _currentTool;
        private Dictionary<ToolType, ToolBase> _toolsByType;

        public event Action<ToolType> OnToolChanged;

        public ToolBase CurrentTool => _currentTool;
        public ToolType CurrentToolType => CurrentTool.Data.type;
        public List<ToolSO> AllToolData => allTools.Select(tool => tool.Data).ToList();

        private void Awake()
        {
            _toolsByType = BuildToolsByType();
            
            ChangeTool(ToolType.GrabTool);
        }

        public void SelectZone(ZoneSO zoneType)
        {
            ChangeTool(ToolType.ZonesTool);
            var zoneTool = (ZoneTool)_toolsByType[ToolType.ZonesTool];
            zoneTool.SetZoneType(zoneType);
        }

        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeTool(ToolType.GrabTool);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeTool(ToolType.AvailableTilesTool);
            }
        }

        public void ChangeTool(ToolType newToolType)
        {
            if (_currentTool != null && _currentTool.Data.type == newToolType) return;

            _currentTool?.OnDeselect();
            _currentTool = _toolsByType[newToolType];
            _currentTool?.OnSelect();

            if (_currentTool != null)
            {
                OnToolChanged?.Invoke(_currentTool.Data.type);
            }
        }

        private Dictionary<ToolType, ToolBase> BuildToolsByType()
        {
            return allTools.ToDictionary(tool => tool.Data.type, tool => tool);
        }
    }
}