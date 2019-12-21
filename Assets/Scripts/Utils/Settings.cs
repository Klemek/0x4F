using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Utils
{
    public class Settings : MonoBehaviour
    {
        [Header("References")]
        public Camera mainCamera;
        public AudioMixer masterMixer;
        
        #region Display
        
        public EnumSettingsEntry<FullScreenMode> displayMode = new EnumSettingsEntry<FullScreenMode>("Display_Mode", FullScreenMode.FullScreenWindow);
        
        public IntSettingsEntry displayWidth = new IntSettingsEntry("Display_Width", 800);
        
        public IntSettingsEntry displayHeight = new IntSettingsEntry("Display_Height", 600);
        
        #endregion
        
        #region Sound
        
        public IntSettingsEntry globalSound = new IntSettingsEntry("Sound_Global", 100);
        
        public IntSettingsEntry musicSound = new IntSettingsEntry("Sound_Music", 100);
        
        // ReSharper disable once InconsistentNaming
        public IntSettingsEntry sfxSound = new IntSettingsEntry("Sound_SFX", 100);
        
        // ReSharper disable once InconsistentNaming
        public IntSettingsEntry uiSound = new IntSettingsEntry("Sound_UI", 100);

        #endregion
        
        #region Camera
        
        public FloatSettingsEntry cameraSensitivity = new FloatSettingsEntry("Camera_Sensitivity", 1f);

        // ReSharper disable once InconsistentNaming
        public IntSettingsEntry cameraFOV = new IntSettingsEntry("Camera_FOV", 60);

        #endregion

        #region Controls
        
        public EnumSettingsEntry<InputManager.InputMode> inputMode = new EnumSettingsEntry<InputManager.InputMode>("Input_Mode", InputManager.InputMode.Auto);

        public Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
        
        #endregion

        private readonly List<UnityAction> _loadedListeners = new List<UnityAction>();

        private bool _loaded;

        private List<ISettingEntry> AllEntries => GetType()
            .GetFields()
            .Where(f => typeof(ISettingEntry).IsAssignableFrom(f.FieldType))
            .Select(f => (ISettingEntry) f.GetValue(this))
            .ToList();
        
        public void Awake()
        {
            LoadSettings();
            InputManager.Register(this);
            StartCoroutine(InputManager.JoystickCheck());
        }

        public void Start()
        {
            ApplySettings();
        }

        public void AddLoadedListener(UnityAction l)
        {
            _loadedListeners.Add(l);
            if (_loaded)
                l();
        }

        private void LoadSettings()
        {
            AllEntries.ForEach(e =>
            {
                e.AddListener(ApplySettings);
                e.Load();
            });
            LoadKeys();
            _loaded = true;
            _loadedListeners.ForEach(l => l());
        }

        public void LoadKeys()
        {
            foreach (var keyName in InputManager.BindingsKeys)
            {
                var savedName = "Key_" + keyName;
                keyBindings[keyName] = PlayerData.GetKeyCode(savedName, InputManager.GetDefaultKey(keyName));
            }
        }
        
        public void ResetSettings()
        {
            AllEntries.ForEach(e => e.Reset());
            ResetKeys();
        }
        
        public void ResetKeys()
        {
            foreach (var keyName in InputManager.BindingsKeys)
            {
                keyBindings[keyName] = InputManager.GetDefaultKey(keyName);
                SetKeyBinding(keyName, keyBindings[keyName]);
            }
        }

        public void SetKeyBinding(string keyName, KeyCode newKey)
        {
            keyBindings[keyName] = newKey;
            if (newKey == InputManager.GetDefaultKey(keyName))
            {
                PlayerData.Reset("Key_" + keyName);
            }
            else
            {
                PlayerData.SetKeyCode("Key_" + keyName, newKey);
            }
            PlayerData.Save();
        }

        private void ApplySettings()
        {
            if(mainCamera)
                mainCamera.fieldOfView = cameraFOV.Value;
            if (!masterMixer) return;
            masterMixer.SetFloat("Master", GameUtils.VolumeToDecibels(globalSound.Value));
            masterMixer.SetFloat("Music", GameUtils.VolumeToDecibels(musicSound.Value));
            masterMixer.SetFloat("SFX", GameUtils.VolumeToDecibels(sfxSound.Value));
            masterMixer.SetFloat("UI", GameUtils.VolumeToDecibels(uiSound.Value));
            if (displayMode.Value != FullScreenMode.Windowed)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, displayMode.Value);
            }
            else
            {
                Screen.SetResolution(displayWidth.Value, displayHeight.Value, displayMode.Value);
            }
            
        }

        private interface ISettingEntry
        {
            void AddListener(Action a);
            void Load();
            void Reset();
        }
        
        public abstract class SettingsEntry<T> : ISettingEntry
        {
            private T _defaultValue;
            protected T value;
            protected string id;
            private Action _listener;

            public T Value
            {
                get => value;
                set
                {
                    this.value = value;
                    SetPlayerData();
                    PlayerData.Save();
                    _listener?.Invoke();
                }
            }

            private protected abstract void SetPlayerData();
            public void AddListener(Action a)
            {
                _listener = a;
            }

            public abstract void Load();

            protected SettingsEntry(string entryId, T defaultValue)
            {
                id = entryId;
                _defaultValue = defaultValue;
                value = _defaultValue;
            }

            public void Reset()
            {
                Value = _defaultValue;
            }
        }

        public class FloatSettingsEntry : SettingsEntry<float>
        {
            private protected override void SetPlayerData()
            {
                PlayerData.SetFloat(id, value);
            }

            public override void Load()
            {
                value = PlayerData.GetFloat(id, value);
            }

            public FloatSettingsEntry(string entryId, float defaultValue) : base(entryId, defaultValue)
            {
            }
        }

        public class IntSettingsEntry : SettingsEntry<int>
        {
            private protected override void SetPlayerData()
            {
                PlayerData.SetInt(id, value);
            }

            public override void Load()
            {
                value = PlayerData.GetInt(id, value);
            }

            public IntSettingsEntry(string entryId, int defaultValue) : base(entryId, defaultValue)
            {
            }
        }

        public class EnumSettingsEntry<T> : SettingsEntry<T> where T : struct, Enum
        {
            private protected override void SetPlayerData()
            {
                PlayerData.SetEnumValue(id, value);
            }

            public override void Load()
            {
                value = PlayerData.GetEnumValue(id, value);
            }

            public EnumSettingsEntry(string entryId, T defaultValue) : base(entryId, defaultValue)
            {
            }
        }
    }

    
}