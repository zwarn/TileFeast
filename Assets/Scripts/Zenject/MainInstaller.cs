using Board;
using Core;
using Piece.hand;
using Piece.Supply;
using Rules;
using Scenario;
using UnityEngine;
using UnityEngine.Serialization;

namespace Zenject
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private HandController handController;
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceSupplyController pieceSupplyController;
        [FormerlySerializedAs("scoreController")] [SerializeField] private RulesController rulesController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private GameController gameController;
        [SerializeField] private HighlightController highlightController;

        public override void InstallBindings()
        {
            Container.Bind<HandController>().FromInstance(handController);
            Container.Bind<BoardController>().FromInstance(boardController);
            Container.Bind<PieceSupplyController>().FromInstance(pieceSupplyController);
            Container.Bind<RulesController>().FromInstance(rulesController);
            Container.Bind<ScenarioController>().FromInstance(scenarioController);
            Container.Bind<GameController>().FromInstance(gameController);
            Container.Bind<HighlightController>().FromInstance(highlightController);
        }
    }
}