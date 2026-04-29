using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Pieces.Supply;
using Placeables.BoardExpansions;
using Placeables.PersonalRulePlacements;
using Roguelike;
using Rules;
using Scenarios;
using Tools;
using UnityEngine;
using Zenject;

namespace Modes
{
    public class RoguelikeModeController : MonoBehaviour
    {
        [Inject] private ScenarioController _scenarioController;
        [Inject] private RulesController _rulesController;
        [Inject] private PieceSupplyController _pieceSupplyController;
        [Inject] private GameController _gameController;
        [Inject] private BoardExpansionPreviewSettings _boardExpansionSettings;
        [Inject] private PersonalRulePlacementSettings _personalRuleSettings;

        public event Action<int> OnHealthChanged;
        public event Action<bool> OnEndTurnAllowedChanged;
        public event Action<RoguelikeDraftGroup> OnDraftGroupAvailable;
        public event Action<int> OnPendingDraftCountChanged;
        public event Action OnGameOver;

        private RoguelikeRunSO _config;
        private int _currentHealth;
        private readonly List<RoguelikeDraftGroup> _draftGroups = new();
        private bool _active;

        public int CurrentHealth => _currentHealth;
        public int PendingDraftCount => _draftGroups.Count(g => !g.IsResolved);

        private void OnEnable()
        {
            _pieceSupplyController.OnItemAdded += HandleSupplyChanged;
            _pieceSupplyController.OnItemRemoved += HandleSupplyChanged;
            _pieceSupplyController.OnItemsReplaced += HandleSupplyReplaced;
        }

        private void OnDisable()
        {
            _pieceSupplyController.OnItemAdded -= HandleSupplyChanged;
            _pieceSupplyController.OnItemRemoved -= HandleSupplyChanged;
            _pieceSupplyController.OnItemsReplaced -= HandleSupplyReplaced;
        }

        public void StartRun(RoguelikeRunSO config)
        {
            _config = config;
            _active = true;
            _currentHealth = config.startingHealth;
            _scenarioController.LoadScenario(config.startingScenario, addProceduralItems: false);
            OnHealthChanged?.Invoke(_currentHealth);
            GenerateAndShowDrafts();
        }

        // Called by the draft panel when the player picks an offer from a group.
        public void PickOffer(RoguelikeDraftGroup group, int offerIdx)
        {
            if (group == null || group.IsResolved) return;
            if (offerIdx < 0 || offerIdx >= group.Options.Count) return;

            group.Options[offerIdx].Apply(CreateApplyContext());
            group.Resolve(offerIdx);

            NotifyPendingCount();
            UpdateEndTurnState();
            ShowNextPendingDraft(skip: group);
        }

        // Called by the draft panel's Back button; dismisses the current group temporarily.
        public void PostponeDraft(RoguelikeDraftGroup group)
        {
            ShowNextPendingDraft(skip: group);
        }

        // Called by the HUD's "Pending Choices" button to surface pending drafts again.
        public void ShowNextPendingDraft() => ShowNextPendingDraft(skip: null);

        public void EndTurn()
        {
            if (!IsEndTurnAllowed()) return;

            var sadCount = _rulesController.LastResult.SadCount;
            var supplyPieceCount = _pieceSupplyController.Items.OfType<PlaceablePiece>().Count();
            _currentHealth -= sadCount + supplyPieceCount;
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                _active = false;
                OnGameOver?.Invoke();
                return;
            }

            GenerateAndShowDrafts();
        }

        public bool IsEndTurnAllowed()
        {
            if (!_active) return false;
            if (!_draftGroups.All(g => g.IsResolved)) return false;
            if (_pieceSupplyController.Items.Any(i => i is not PlaceablePiece)) return false;
            return true;
        }

        private void GenerateAndShowDrafts()
        {
            _draftGroups.Clear();

            if (_config.offerPool.Count < 3)
            {
                Debug.LogWarning("[RoguelikeModeController] offerPool has fewer than 3 entries.");
                return;
            }

            for (var i = 0; i < _config.draftGroupsPerTurn; i++)
                _draftGroups.Add(new RoguelikeDraftGroup(PickThreeRandom(_config.offerPool)));

            NotifyPendingCount();
            ShowNextPendingDraft(skip: null);
            UpdateEndTurnState();
        }

        private void ShowNextPendingDraft(RoguelikeDraftGroup skip)
        {
            foreach (var group in _draftGroups)
            {
                if (group == skip) continue;
                if (!group.IsResolved)
                {
                    OnDraftGroupAvailable?.Invoke(group);
                    return;
                }
            }
            // No other pending groups — panel stays hidden.
            // Player uses the HUD "Pending Choices" button to resurface the skipped group.
        }

        private void NotifyPendingCount() => OnPendingDraftCountChanged?.Invoke(PendingDraftCount);

        private void UpdateEndTurnState() => OnEndTurnAllowedChanged?.Invoke(IsEndTurnAllowed());

        private void HandleSupplyChanged(IPlaceable _) => UpdateEndTurnState();
        private void HandleSupplyReplaced(List<IPlaceable> _) => UpdateEndTurnState();

        private RoguelikeApplyContext CreateApplyContext() => new()
        {
            GameController = _gameController,
            SupplyController = _pieceSupplyController,
            BoardExpansionSettings = _boardExpansionSettings,
            PersonalRuleSettings = _personalRuleSettings,
        };

        private static List<RoguelikeOfferSO> PickThreeRandom(List<RoguelikeOfferSO> pool)
        {
            return pool.OrderBy(_ => UnityEngine.Random.value).Take(3).ToList();
        }
    }
}
