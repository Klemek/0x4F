using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    public class Score : MonoBehaviour
    {
        public int score;
        private int _toAdd;
        private int _target;
        
        [Header("References")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI updateText;
    
        [Header("Score Update")]
        public float scoreUpdateDelay = 2;

        private void Start()
        {
            scoreText.text = score.ToString();
            updateText.text = "";
        }

        public void AddPoints(int value)
        {
            _target += value;
            _toAdd += value;
            updateText.text = "+" + _toAdd;
            StopCoroutine(nameof(AddScoreSmooth));
            StartCoroutine(nameof(AddScoreSmooth));
        }

        public void FreezeScore()
        {
            StopCoroutine(nameof(AddScoreSmooth));
            score = _target;
            scoreText.text = score.ToString();
            _toAdd = 0;
            updateText.text = "";
        }
    
        private IEnumerator AddScoreSmooth()
        {
            while (Mathf.Abs(score - _target) > 5)
            {
                score = Mathf.CeilToInt(Mathf.Lerp(score, _target, Time.unscaledDeltaTime / scoreUpdateDelay));
                scoreText.text = score.ToString();
                yield return null;
            }

            FreezeScore();
        }

        private void OnValidate()
        {
            scoreText.text = score.ToString();
        }
    }
}
