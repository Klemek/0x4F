using UnityEngine;

namespace GameObjects
{
    public class Orb : MonoBehaviour
    {
       

        public float speed;
        public float angleRange;
        public float maxRemaining;

        public Color color;
        public int points;
    
        private GameManager _gameManager;
        public ParticleSystem breakingParticles;
        public SimpleSFX breakingSound;

        private float _remaining;
        private float _angle;

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        public void UpdateColor()
        {
            GetComponent<Renderer>().material.color = color;
            var trail = GetComponentInChildren<TrailRenderer>();
            trail.startColor = color;
            trail.endColor = color;
            GetComponentInChildren<Light>().color = color;
        }

        private void FixedUpdate()
        {
            _remaining -= Time.deltaTime;
            if (_remaining < float.Epsilon)
            {
                _remaining = Random.Range(0, maxRemaining);
                _angle = Random.Range(-angleRange, angleRange);
            }
            transform.Rotate(Vector3.up, _angle);
            transform.Translate(Time.deltaTime * speed * Vector3.forward);
            if (Mathf.Abs(transform.position.x) >= _gameManager.size || Mathf.Abs(transform.position.z) >= _gameManager.size)
            {
                transform.Rotate(Vector3.up, 180);
            }
        }

    
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            var particles = Instantiate(breakingParticles, transform.position, Quaternion.identity);
            particles.transform.parent = transform.parent;
            particles.GetComponent<Renderer>().material.color = color;
            var sfx = Instantiate(breakingSound, transform.position, Quaternion.identity);
            sfx.transform.parent = transform.parent;
            Destroy(gameObject);
            _gameManager.AddPoints(points);
        }
    }
}