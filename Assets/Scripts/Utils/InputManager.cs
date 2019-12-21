using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static class InputManager
    {
        public enum InputMode
        {
            Auto,
            Keyboard,
            Joystick
        }
        
        private static readonly Dictionary<string, KeyCode> DefaultBindings = new Dictionary<string, KeyCode>
        {
            {"Forward", KeyCode.W},
            {"Backward", KeyCode.S},
            {"Left", KeyCode.A},
            {"Right", KeyCode.D},
            {"Sprint", KeyCode.LeftShift},
            {"Jump", KeyCode.Space},
            {"Cancel", KeyCode.Escape},
            {"Menu", KeyCode.Escape}
        };

        private static readonly Dictionary<string, KeyCode> DefaultBindingsAzerty = new Dictionary<string, KeyCode>
        {
            {"Forward", KeyCode.Z},
            {"Left", KeyCode.Q}
        };

        public static readonly string[] BindingsKeys = DefaultBindings.Keys.ToArray();
        
        private static readonly Dictionary<string, Tuple<string, string>> Axes = new Dictionary<string, Tuple<string, string>>
        {
            {"Vertical", new Tuple<string, string>("Backward", "Forward")},
            {"Horizontal", new Tuple<string, string>("Left", "Right")}
        };

        private static bool _wasAzerty = KeyboardLayout.IsAzerty;

        public static bool UseJoystick => _settings != null && (_settings.inputMode.Value == InputMode.Joystick || _settings.inputMode.Value == InputMode.Auto && _joystickConnected);

        private static bool _joystickConnected = IsJoystickConnected();
        private static readonly List<UnityAction<bool>> JoystickListeners = new List<UnityAction<bool>>();
        
        private static readonly List<UnityAction<bool>> KeyBoardListeners = new List<UnityAction<bool>>();

        private static Settings _settings;

        public static void Register(Settings settings)
        {
            _settings = settings;
        }
        
        public static void AddJoystickListener(UnityAction<bool> action)
        {
            JoystickListeners.Add(action);
        }

        public static void AddKeyboardListener(UnityAction<bool> action)
        {
            KeyBoardListeners.Add(action);
        }
        
        private static bool IsJoystickConnected()
        {
            return Input.GetJoystickNames().Any(name => !string.IsNullOrEmpty(name));
        }
    
        public static IEnumerator JoystickCheck()
        {
            while (true)
            {
                var connected = IsJoystickConnected();
                if (connected ^ _joystickConnected)
                {
                    _joystickConnected = connected;
                    if(_settings.inputMode.Value == InputMode.Auto)
                        JoystickListeners.ForEach(a => a(connected));
                }
            
                KeyboardLayout.DetectKeyboardLayout();
                if (KeyboardLayout.IsAzerty ^ _wasAzerty)
                {
                    _wasAzerty = KeyboardLayout.IsAzerty;
                    _settings.LoadKeys();
                    KeyBoardListeners.ForEach(a => a(_wasAzerty));
                }

                yield return new WaitForSecondsRealtime(.5f);
            }
        }

        public static KeyCode GetDefaultKey(string keyName)
        {
            if (KeyboardLayout.IsAzerty && DefaultBindingsAzerty.ContainsKey(keyName))
            {
                return DefaultBindingsAzerty[keyName];
            }
            return DefaultBindings[keyName];
        }

        public static bool GetKey(string keyName)
        {
            if (!_settings) return false;
            if (UseJoystick) return Input.GetButton(keyName);
            return _settings.keyBindings.ContainsKey(keyName) && Input.GetKey(_settings.keyBindings[keyName]);
        }
    
        public static bool GetKeyDown(string keyName)
        {
            if (!_settings) return false;
            if (UseJoystick) return Input.GetButtonDown(keyName);
            return _settings.keyBindings.ContainsKey(keyName) && Input.GetKeyDown(_settings.keyBindings[keyName]);
        }

        private static float GetAxisRaw(string axisName)
        {
            if (!_settings) return 0f;
            if (UseJoystick)
                return Input.GetAxisRaw(axisName);
            if (!Axes.ContainsKey(axisName))
                return 0f;
            var negative = GetKey(Axes[axisName].Item1);
            var positive = GetKey(Axes[axisName].Item2);
            if (!(negative ^ positive)) // none or both
                return 0f;
            return negative ? -1f : 1f;
        }

        public static Vector3 GetMoveInput()
        {
            var move = new Vector3(GetAxisRaw("Horizontal"),0f,GetAxisRaw("Vertical"));
            return Vector3.ClampMagnitude(move, 1);
        }

        public static float GetCameraXAxis()
        {
            return GetCameraAxis("X");
        }

        public static float GetCameraYAxis()
        {
            return GetCameraAxis("Y");
        }

        private static float GetCameraAxis(string axisName)
        {
            if (!_settings) return 0f;
            return (UseJoystick ? Input.GetAxis("Joystick " + axisName) * Time.deltaTime : Input.GetAxisRaw("Mouse " + axisName) * 0.01f ) * _settings.cameraSensitivity.Value;
        }
    
    }
}