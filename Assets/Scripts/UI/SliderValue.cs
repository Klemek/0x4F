using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteInEditMode]
    public class SliderValue : MonoBehaviour
    {
        public TextMeshProUGUI value;
        public int decimals = 2;
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(UpdateText);
            UpdateText();
        }

        private void UpdateText()
        {
            if (_slider)
                UpdateText(_slider.value);
        }

        private void UpdateText(float v)
        {
            value.text = v.ToString("F" + decimals);
        }

        private void OnValidate()
        {
            UpdateText();
        }
    }
}