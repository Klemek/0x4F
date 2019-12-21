using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace UI
{
    [ExecuteInEditMode]
    public class SwitchButton : MonoBehaviour
    {
        private Button _button;
        private TextMeshProUGUI _text;
        
        [SerializeField]
        private bool state;
        public bool State
        {
            get => state;
            set
            {
                ValueChange(value);
                UpdateState();
            }
        }

        private readonly List<UnityAction<bool>> _listeners = new List<UnityAction<bool>>();

        [Header("On")] 
        public string onText = "Yes";
        public Color onColor = Color.white;

        [Header("Off")] 
        public string offText = "No";
        public Color offColor = new Color(1,1,1,.8f);

        private void Start()
        {
            _button = GetComponent<Button>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _button.onClick.AddListener(() => State = !State);
        }

        public void AddListener(UnityAction<bool> action)
        {
            _listeners.Add(action);
        }
        
        private void ValueChange(bool value)
        {
            state = value;
            UpdateState();
            _listeners.ForEach(a => a(value));
        }
        
        private void UpdateState()
        {
            if (!_button) return;
            var buttonColors = _button.colors;
            buttonColors.normalColor = state ? onColor : offColor;
            buttonColors.selectedColor = state ? onColor : offColor;
            _button.colors = buttonColors;
            _text.text = state ? onText : offText;
        }

        public void SetEnabled(bool v)
        {
            _button.interactable = v;
        }

        private void OnValidate()
        {
            UpdateState();
        }
    }
}
