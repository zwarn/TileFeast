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
            var width = gameState.GridSize.x;
            var height = gameState.GridSize.y;

            var maxSize = Math.Max(width, height);

            camera.orthographicSize = 1f + maxSize / 2f;
            camera.transform.position = new Vector3(width, height, camera.transform.position.z);
        }
    }
}