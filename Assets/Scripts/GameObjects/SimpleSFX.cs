using UnityEngine;
using Utils;

namespace GameObjects
{
    // ReSharper disable once InconsistentNaming
    public class SimpleSFX : MonoBehaviour
    {
        public AudioClip[] pool;
        public float pitchRange = .1f;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            GameUtils.PlayRandomSFX(_audioSource, pitchRange, pool);
        }
        
        private void LateUpdate()
        {
            if(!_audioSource.isPlaying)
                Destroy(gameObject);
        }
    }
}