using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI
{
    [ExecuteInEditMode]
    public class KeyButton : MonoBehaviour
    {
       
        [SerializeField]
        private KeyCode key = KeyCode.None;

        public KeyCode Key
        {
            get => key;
            set
            {
                key = value;
                UpdateButton();
            }
        }

        public bool listening;
        public string listeningText = "(Enter key)";
        public bool onlyKeyboard;

        private TextMeshProUGUI _text;
        private Button _button;
        
        private readonly List<UnityAction> _startListener = new List<UnityAction>();
        private readonly List<UnityAction> _endListener = new List<UnityAction>();

        private void Start()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                listening = true;
                UpdateButton();
                _startListener.ForEach(l => l());
            });
            UpdateButton();
        }

        public void AddStartListener(UnityAction l)
        {
            _startListener.Add(l);
        }
        
        public void AddEndListener(UnityAction l)
        {
            _endListener.Add(l);
        }

        public void Cancel()
        {
            listening = false;
            UpdateButton();
        }
        
        private void LateUpdate()
        {
            if (!listening || !Input.anyKeyDown) return;
            key = KeyCode.None;
            if(!InputManager.GetKeyDown("Cancel"))
            {
                foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (!Input.GetKeyDown(keyCode)) continue;
                    if (onlyKeyboard && keyCode.ToString().StartsWith("Joystick")) return;
                    key = keyCode;
                    break;
                }
            }
            _endListener.ForEach(l => l());
            Cancel();
        }

        private void UpdateButton()
        {
            if(_text)
                _text.text = listening ? listeningText : (key == KeyCode.None ? "---" : GameUtils.Prettify(key.ToString()));
        }

        private void OnValidate()
        {
            UpdateButton();
        }
    }
}
