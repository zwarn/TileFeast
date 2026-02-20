using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scenarios
{
    public class ScenarioPersistence : MonoBehaviour
    {
        [Inject] private GameController _gameController;
        [Inject] private ScenarioController _scenarioController;

        [SerializeField] private string defaultSavePath = "Assets/ScriptableObjects/Scenario/";

        public ScenarioSO Save(string scenarioName)
        {
#if UNITY_EDITOR
            var path = $"{defaultSavePath}{scenarioName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioSO>(path);

            if (existing != null)
            {
                SaveToExistingScenario(existing);
                return existing;
            }

            return SaveAsNewScenario(scenarioName);
#else
            Debug.LogWarning("Saving scenarios is not supported in builds");
            return null;
#endif
        }

#if UNITY_EDITOR
        private ScenarioSO SaveAsNewScenario(string fileName)
        {
            var scenario = ScriptableObject.CreateInstance<ScenarioSO>();
            PopulateScenarioFromState(scenario);

            var path = $"{defaultSavePath}{fileName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(scenario, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Scenario saved to: {path}");
            return scenario;
        }

        private void SaveToExistingScenario(ScenarioSO scenario)
        {
            if (scenario == null)
            {
                Debug.LogError("Cannot save to null scenario");
                return;
            }

            PopulateScenarioFromState(scenario);

            EditorUtility.SetDirty(scenario);
            AssetDatabase.SaveAssets();

            Debug.Log($"Scenario updated: {scenario.name}");
        }

        private void PopulateScenarioFromState(ScenarioSO scenario)
        {
            var state = _gameController.CurrentState;

            scenario.gridSize = state.GridSize;
            scenario.blockedPositions = state.BlockedPositions.ToList();
            scenario.scoreRules = state.ScoreRules.ToList();
            scenario.placementRules = state.PlacementRules.ToList();
            scenario.zones = state.Zones.Select(z => z.Clone()).ToList();

            SetAvailablePieces(scenario, state.AvailablePieces);
            SetLockedPieces(scenario, state.PlacedPieces.Where(p => p.IsLocked()).ToList());
        }

        private void SetAvailablePieces(ScenarioSO scenario, List<Piece> pieces)
        {
            var pieceSOs = new List<PieceSO>();

            foreach (var piece in pieces)
            {
                if (piece.sourceSO != null)
                {
                    pieceSOs.Add(piece.sourceSO);
                }
                else
                {
                    Debug.LogWarning($"Piece with shape {piece.shape.Count} tiles has no source PieceSO - skipping");
                }
            }

            var field = typeof(ScenarioSO).GetField("availablePieces",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(scenario, pieceSOs);
        }

        private void SetLockedPieces(ScenarioSO scenario, List<PlacedPiece> placedPieces)
        {
            var grouped = placedPieces
                .Where(p => p.Piece.sourceSO != null)
                .GroupBy(p => p.Piece.sourceSO)
                .ToList();

            var lockedPieceDataList = new List<LockedPieceData>();

            foreach (var group in grouped)
            {
                var data = new LockedPieceData
                {
                    type = new LockedPieceType { piece = group.Key },
                    instances = group.Select(p => new LockedPieceInstance
                    {
                        position = p.Position,
                        rotation = p.Rotation
                    }).ToList()
                };
                lockedPieceDataList.Add(data);
            }

            var lockedPieceList = new LockedPieceList();
            var dataField = typeof(LockedPieceList).GetField("data",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dataField?.SetValue(lockedPieceList, lockedPieceDataList);

            var lockedField = typeof(ScenarioSO).GetField("lockedPieces",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lockedField?.SetValue(scenario, lockedPieceList);
        }
#endif
    }
}
