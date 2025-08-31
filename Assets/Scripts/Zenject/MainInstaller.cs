using Board;
using Hand;
using Piece.controller;
using Score;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private HandController handController;
    [SerializeField] private BoardController boardController;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private PieceSupplyController pieceSupplyController;
    [SerializeField] private ScoreController scoreController;

    public override void InstallBindings()
    {
        Container.Bind<HandController>().FromInstance(handController);
        Container.Bind<BoardController>().FromInstance(boardController);
        Container.Bind<InteractionController>().FromInstance(interactionController);
        Container.Bind<PieceSupplyController>().FromInstance(pieceSupplyController);
        Container.Bind<ScoreController>().FromInstance(scoreController);
    }
}