using Board;
using Core;
using Hand.Tool;
using Piece.Supply;
using Rules;
using Scenario;
using UnityEngine;

namespace Zenject
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private ToolController toolController;
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceSupplyController pieceSupplyController;
        [SerializeField] private RulesController rulesController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private GameController gameController;
        [SerializeField] private HighlightController highlightController;

        public override void InstallBindings()
        {
            Container.Bind<ToolController>().FromInstance(toolController);
            Container.Bind<BoardController>().FromInstance(boardController);
            Container.Bind<PieceSupplyController>().FromInstance(pieceSupplyController);
            Container.Bind<RulesController>().FromInstance(rulesController);
            Container.Bind<ScenarioController>().FromInstance(scenarioController);
            Container.Bind<GameController>().FromInstance(gameController);
            Container.Bind<HighlightController>().FromInstance(highlightController);
        }
    }
}