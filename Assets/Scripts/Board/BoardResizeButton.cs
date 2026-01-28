using Core;
using UnityEngine;
using Zenject;

namespace Board
{
    public class BoardResizeButton : MonoBehaviour
    {

        [Inject] private GameController _gameController;

        [SerializeField] private Vector2Int deltaSize;
        [SerializeField] private Vector2Int translate;
        
        public void OnClick()
        {
            _gameController.ChangeBoardSize(deltaSize, translate);
        }
        
        
    }
}