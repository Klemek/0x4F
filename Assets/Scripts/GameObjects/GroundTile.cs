using UnityEngine;
using Random = UnityEngine.Random;

namespace GameObjects
{
    public class GroundTile : MonoBehaviour
    {
        public ParticleSystem destroyParticles;
        public SimpleSFX destroySound;
        public int random;
        private GameManager _gameManager;
        public bool locked;
        private string _basename;

        // Start is called before the first frame update
        private void Start()
        {
            locked = false;
            random = Random.Range(0, 100);
            _gameManager = GameManager.Instance;
            _basename = "(" + random + ")" + transform.name;
            name = _basename;
        }

        // Update is called once per frame
        private void Update()
        {
            if (locked || !gameObject.activeInHierarchy || !_gameManager.randoms.Contains(random)) return;
            var particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
            particles.transform.parent = transform.parent;
            var sound = Instantiate(destroySound, transform.position, Quaternion.identity);
            sound.transform.parent = transform.parent;
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            locked = true;
            name = "(P)" + _basename;
        }

        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            locked = false;
            name = _basename;
        }
    }
}