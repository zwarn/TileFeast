using Board;
using Board.Zone;
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
        [SerializeField] private ZoneController zoneController;
        [SerializeField] private CameraController cameraController;

        public override void InstallBindings()
        {
            Container.Bind<ToolController>().FromInstance(toolController);
            Container.Bind<BoardController>().FromInstance(boardController);
            Container.Bind<PieceSupplyController>().FromInstance(pieceSupplyController);
            Container.Bind<RulesController>().FromInstance(rulesController);
            Container.Bind<ScenarioController>().FromInstance(scenarioController);
            Container.Bind<GameController>().FromInstance(gameController);
            Container.Bind<HighlightController>().FromInstance(highlightController);
            Container.Bind<ZoneController>().FromInstance(zoneController);
            Container.Bind<CameraController>().FromInstance(cameraController);
        }
    }
}