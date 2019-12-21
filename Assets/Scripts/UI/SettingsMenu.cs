using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("General")] public Settings settings;
        public Button resetButton;

        public bool locked;

        [Header("Audio")] public Slider global;
        public Slider music;
        public Slider sfx;
        public Slider ui;

        [Header("Display")] public TMP_Dropdown displayMode;
        public TMP_Dropdown displayResolution;

        [Header("Camera")] public Slider sensibility;
        public Slider fov;

        [Header("Controls")] public TMP_Dropdown inputMode;

        [Header("Keys")] public RectTransform keysPanel;
        public RectTransform templateKey;
        private KeyButton[] _keyButtons;

        private bool _eventLock;

        private void Start()
        {
            CreateKeyButtons();
            inputMode.AddOptions(Enum.GetNames(typeof(InputManager.InputMode)).Select(GameUtils.Prettify).ToList());
            displayMode.AddOptions(Enum.GetNames(typeof(FullScreenMode)).Select(GameUtils.Prettify).ToList());
            displayResolution.AddOptions(GameUtils.CommonResolutions.Select(GameUtils.GetResolutionName).ToList());

            settings.AddLoadedListener(() =>
            {
                LoadValues();
                LoadEvents();
            });

            resetButton.onClick.AddListener(() =>
            {
                settings.ResetSettings();
                LoadValues();
            });

            InputManager.AddKeyboardListener(azerty => LoadValues());
            InputManager.AddJoystickListener(joystick => LoadValues());
        }

        private void LoadValues()
        {
            // display
            LoadResolution();
            _eventLock = true;
            // audio
            global.value = settings.globalSound.Value;
            music.value = settings.musicSound.Value;
            sfx.value = settings.sfxSound.Value;
            ui.value = settings.uiSound.Value;
            // camera
            sensibility.value = settings.cameraSensitivity.Value;
            fov.value = settings.cameraFOV.Value;
            // controls
            inputMode.value = (int) settings.inputMode.Value;
            LoadKeys();
            _eventLock = false;
        }

        private IEnumerator LoadResolutionAsync()
        {
            yield return new WaitForSecondsRealtime(.25f);
            LoadResolution();
        }

        private void LoadResolution()
        {
            _eventLock = true;
            displayMode.value = (int) Screen.fullScreenMode;
            displayResolution.value = GameUtils.GetCurrentResolution();
            displayResolution.interactable = settings.displayMode.Value == FullScreenMode.Windowed;
            _eventLock = false;
        }

        private void LoadEvents()
        {
            //display
            displayMode.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.displayMode.Value = (FullScreenMode) v;
                StartCoroutine(LoadResolutionAsync());
            });
            displayResolution.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                if (v > 0)
                {
                    var res = GameUtils.CommonResolutions[v];
                    settings.displayWidth.Value = res[0];
                    settings.displayHeight.Value = res[1];
                }
                StartCoroutine(LoadResolutionAsync());
            });

            //audio
            global.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.globalSound.Value = (int) v;
            });
            music.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.musicSound.Value = (int) v;
            });
            sfx.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.sfxSound.Value = (int) v;
            });
            ui.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.uiSound.Value = (int) v;
            });
            // camera
            sensibility.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.cameraSensitivity.Value = v;
            });
            fov.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.cameraFOV.Value = (int) v;
            });
            // controls
            inputMode.onValueChanged.AddListener(v =>
            {
                if (_eventLock) return;
                settings.inputMode.Value = (InputManager.InputMode) v;
                LoadKeys();
            });
        }

        private void CreateKeyButtons()
        {
            _keyButtons = new KeyButton[InputManager.BindingsKeys.Length];
            var height = templateKey.sizeDelta.y;
            for (var i = 0; i < InputManager.BindingsKeys.Length; i++)
            {
                var newKey = Instantiate(templateKey, keysPanel);
                newKey.pivot = templateKey.pivot;
                newKey.localPosition = templateKey.localPosition - new Vector3(0, i * height, 0);
                newKey.GetComponentInChildren<TextMeshProUGUI>().text = InputManager.BindingsKeys[i];
                newKey.name = "Key " + InputManager.BindingsKeys[i];
                _keyButtons[i] = newKey.GetComponentInChildren<KeyButton>();
                var id = i;
                _keyButtons[i].AddStartListener(() =>
                {
                    locked = true;
                    for (var j = 0; j < _keyButtons.Length; j++)
                        if (j != id)
                            _keyButtons[j].Cancel();
                });
                _keyButtons[i].AddEndListener(() =>
                {
                    locked = false;
                    settings.SetKeyBinding(InputManager.BindingsKeys[id], _keyButtons[id].Key);
                });
            }

            Destroy(templateKey.gameObject);
        }

        private void LoadKeys()
        {
            for (var i = 0; i < InputManager.BindingsKeys.Length; i++)
            {
                _keyButtons[i].Key = settings.keyBindings[InputManager.BindingsKeys[i]];
                _keyButtons[i].GetComponent<Button>().interactable = !InputManager.UseJoystick;
            }
        }
    }
}