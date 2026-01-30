using System;
using UnityEngine;
using Zenject;

namespace Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += OnStateChange;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= OnStateChange;
        }

        private void OnStateChange(GameState gameState)
        {
            UpdateState(gameState.GridSize);
        }

        public void HandleBoardResize(Vector2Int gridSize)
        {
            UpdateState(gridSize);
        }


        private void UpdateState(Vector2Int gridSize)
        {
            var width = gridSize.x;
            var height = gridSize.y;

            var maxSize = Math.Max(width, height);

            // this is overwritten by the pixel perfect camera and needs to be rethought if we want boards with sizes above 10
            camera.orthographicSize = 1f + maxSize / 2f;
            camera.transform.position = new Vector3(width, height, camera.transform.position.z);
        }
    }
}