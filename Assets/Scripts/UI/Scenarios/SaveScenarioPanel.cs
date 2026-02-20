using System;
using Core;
using Scenarios;
using TMPro;
using Tools;
using UnityEngine;
using Zenject;

namespace UI.Scenarios
{
    public class SaveScenarioPanel : MonoBehaviour
    {
        [SerializeField] private Transform visualParent;
        [SerializeField] private TMP_InputField saveName;

        [Inject] private ToolController _toolController;
        [Inject] private ScenarioController _scenarioController;
        [Inject] private GameController _gameController;
        [Inject] private ScenarioPersistence _scenarioPersistence;

        private void OnEnable()
        {
            _toolController.OnToolChanged += UpdateVisibility;
            _gameController.OnChangeGameState += OnLoadScenario;
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= UpdateVisibility;
            _gameController.OnChangeGameState -= OnLoadScenario;
        }

        private void UpdateVisibility(ToolType tool)
        {
            visualParent.gameObject.SetActive(tool == ToolType.SaveTool);
        }

        private void OnLoadScenario(GameState newState)
        {
            var currentScenario = _scenarioController.CurrentScenario;

            saveName.text = currentScenario.name;
        }

        public void ReturnAllPieces()
        {
            _gameController.ReturnAllNonLockedPiecesToSupply();
        }

        public void OnSave()
        {
            ReturnAllPieces();
            _scenarioPersistence.Save(saveName.text);
        }
    }
}