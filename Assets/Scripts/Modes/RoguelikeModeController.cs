using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Pieces.Supply;
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

        public event Action<int> OnHealthChanged;
        public event Action<bool> OnEndTurnAllowedChanged;
        public event Action<RoguelikeDraftGroup> OnDraftGroupAvailable;
        public event Action<int> OnPendingDraftCountChanged;
        public event Action OnGameOver;

        private RoguelikeRunSO _config;
        private int _currentHealth;
        private int _turnNumber;
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
            _turnNumber = 0;
            _currentHealth = config.startingHealth;
            _scenarioController.LoadScenario(config.startingScenario, addProceduralItems: false);
            OnHealthChanged?.Invoke(_currentHealth);
            GenerateAndShowDrafts();
        }

        public void PickOffer(RoguelikeDraftGroup group, int offerIdx)
        {
            if (group == null || group.IsResolved) return;
            if (offerIdx < 0 || offerIdx >= group.Options.Count) return;

            var offer = group.Options[offerIdx];

            switch (offer.Type)
            {
                case DraftOfferType.Piece:
                    _pieceSupplyController.AddItem(offer.Placeable);
                    group.Resolve(offerIdx);
                    NotifyPendingCount();
                    UpdateEndTurnState();
                    ShowNextPendingDraft(skip: group);
                    break;

                case DraftOfferType.Placeable:
                    var draft = new DraftPlaceable(
                        offer.Placeable,
                        onPlaced: () =>
                        {
                            group.Resolve(offerIdx);
                            NotifyPendingCount();
                            UpdateEndTurnState();
                            ShowNextPendingDraft(skip: group);
                        },
                        onDiscarded: () => { }
                    );
                    _gameController.PutInHand(draft);
                    break;

                case DraftOfferType.Rule:
                    _gameController.AddEmotionRule(offer.Rule);
                    group.Resolve(offerIdx);
                    NotifyPendingCount();
                    UpdateEndTurnState();
                    ShowNextPendingDraft(skip: group);
                    break;
            }
        }

        public void PostponeDraft(RoguelikeDraftGroup group)
        {
            ShowNextPendingDraft(skip: group);
        }

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

            if (_config.turnPattern == null || _config.turnPattern.Count == 0)
            {
                Debug.LogWarning("[RoguelikeModeController] turnPattern is empty.");
                return;
            }

            var cfg = _config.turnPattern[_turnNumber % _config.turnPattern.Count];

            for (var i = 0; i < cfg.groupCount; i++)
            {
                var offers = cfg.type switch
                {
                    OfferGroupType.Pieces => _config.pieceGenerator != null
                        ? _config.pieceGenerator.GenerateGroup(3, _gameController)
                        : new List<RoguelikeDraftOffer>(),
                    OfferGroupType.Placeables => _config.placeableGenerator != null
                        ? _config.placeableGenerator.GenerateGroup(3, _gameController)
                        : new List<RoguelikeDraftOffer>(),
                    OfferGroupType.Rules => _config.ruleGenerator != null
                        ? _config.ruleGenerator.GenerateGroup(3)
                        : new List<RoguelikeDraftOffer>(),
                    _ => new List<RoguelikeDraftOffer>()
                };

                if (offers.Count > 0)
                    _draftGroups.Add(new RoguelikeDraftGroup(offers, cfg.type));
            }

            _turnNumber++;
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
        }

        private void NotifyPendingCount() => OnPendingDraftCountChanged?.Invoke(PendingDraftCount);
        private void UpdateEndTurnState() => OnEndTurnAllowedChanged?.Invoke(IsEndTurnAllowed());
        private void HandleSupplyChanged(IPlaceable _) => UpdateEndTurnState();
        private void HandleSupplyReplaced(List<IPlaceable> _) => UpdateEndTurnState();
    }
}
