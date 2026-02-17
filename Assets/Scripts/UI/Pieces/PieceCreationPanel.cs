using System.Collections.Generic;
using System.Linq;
using Pieces;
using Tools;
using UnityEngine;
using Zenject;

namespace UI.Pieces
{
    public class PieceCreationPanel : MonoBehaviour
    {
        [SerializeField] private PieceCreationEntry prefab;
        [SerializeField] private Transform entryParent;
        [SerializeField] private Transform visualParent;

        private readonly Dictionary<PieceMatch, PieceCreationEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private ToolController _toolController;
        [Inject] private ShapeTool _shapeTool;

        private void OnEnable()
        {
            _toolController.OnToolChanged += UpdateVisibility;
            _shapeTool.OnMatchingPiecesChanged += UpdateMatching;
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= UpdateVisibility;
            _shapeTool.OnMatchingPiecesChanged -= UpdateMatching;
        }

        private void UpdateMatching(List<PieceMatch> matchingPieces)
        {
            Clear();
            matchingPieces.ForEach(match =>
            {
                var entry = _container.InstantiatePrefabForComponent<PieceCreationEntry>(prefab, entryParent);
                entry.SetData(match);
                _entries.Add(match, entry);
            });
        }

        private void Clear()
        {
            _entries.Values.ToList().ForEach(entry =>  Destroy(entry.gameObject));
            _entries.Clear();
        }

        private void UpdateVisibility(ToolType toolType)
        {
            visualParent.gameObject.SetActive(toolType == ToolType.PiecesTool);
        }

        
    }
}