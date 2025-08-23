using Board;
using Hand;
using Score;
using Shape.controller;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private HandController handController;
    [SerializeField] private BoardController boardController;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private ShapeSupplyController shapeSupplyController;
    [SerializeField] private ScoreController scoreController;

    public override void InstallBindings()
    {
        Container.Bind<HandController>().FromInstance(handController);
        Container.Bind<BoardController>().FromInstance(boardController);
        Container.Bind<InteractionController>().FromInstance(interactionController);
        Container.Bind<ShapeSupplyController>().FromInstance(shapeSupplyController);
        Container.Bind<ScoreController>().FromInstance(scoreController);
    }
}