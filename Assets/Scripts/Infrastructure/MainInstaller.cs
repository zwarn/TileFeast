using Board;
using Cameras;
using Zones;
using Core;
using Tools;
using Pieces;
using Pieces.Supply;
using Rules;
using Scenarios;
using Solver;
using UnityEngine;
using Zenject;

namespace Infrastructure
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private ToolController toolController;
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceSupplyController pieceSupplyController;
        [SerializeField] private RulesController rulesController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private ScenarioPersistence scenarioPersistence;
        [SerializeField] private EditorModeController editorModeController;
        [SerializeField] private GameController gameController;
        [SerializeField] private HighlightController highlightController;
        [SerializeField] private ZoneController zoneController;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private PieceRepository pieceRepository;
        [SerializeField] private SolverRunner solverRunner;
        [Header("Tools")] [SerializeField] private GrabTool grabTool;
        [SerializeField] private ZoneTool zoneTool;
        [SerializeField] private ShapeTool shapeTool;


        public override void InstallBindings()
        {
            Container.Bind<ToolController>().FromInstance(toolController);
            Container.Bind<BoardController>().FromInstance(boardController);
            Container.Bind<PieceSupplyController>().FromInstance(pieceSupplyController);
            Container.Bind<RulesController>().FromInstance(rulesController);
            Container.Bind<ScenarioController>().FromInstance(scenarioController);
            Container.Bind<ScenarioPersistence>().FromInstance(scenarioPersistence);
            Container.Bind<EditorModeController>().FromInstance(editorModeController);
            Container.Bind<GameController>().FromInstance(gameController);
            Container.Bind<HighlightController>().FromInstance(highlightController);
            Container.Bind<ZoneController>().FromInstance(zoneController);
            Container.Bind<CameraController>().FromInstance(cameraController);
            Container.Bind<PieceRepository>().FromInstance(pieceRepository);

            Container.Bind<GrabTool>().FromInstance(grabTool);
            Container.Bind<ZoneTool>().FromInstance(zoneTool);
            Container.Bind<ShapeTool>().FromInstance(shapeTool);

            Container.Bind<SolverRunner>().FromInstance(solverRunner);
        }
    }
}