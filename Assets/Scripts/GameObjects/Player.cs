using UnityEngine;

namespace GameObjects
{
    public class Player : MonoBehaviour
    {
        private GameManager _gameManager;
        private FpsPlayerController _playerController;
    
        // Start is called before the first frame update
        private void Start()
        {
            _gameManager = GameManager.Instance;
            _playerController = GetComponent<FpsPlayerController>();
        }

        public void Teleport(Vector3 position)
        {
            _playerController.Teleport(position);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (_gameManager.paused) return;
                // constraint player
            
            var position = transform.position;
            _playerController.Teleport(new Vector3(
                Mathf.Clamp(position.x, -_gameManager.size+_playerController.PlayerRadius, _gameManager.size-_playerController.PlayerRadius),
                position.y,
                Mathf.Clamp(position.z, -_gameManager.size+_playerController.PlayerRadius, _gameManager.size-_playerController.PlayerRadius)
            ));

            // check for respawn
            if (_playerController.isGrounded || transform.position.y > -_gameManager.levelHeight) return;
            _playerController.ResetHorizontalVelocity();
            _gameManager.RespawnPlayer();
        }
    }
}
