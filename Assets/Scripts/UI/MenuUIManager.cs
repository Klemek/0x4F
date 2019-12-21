using System;
using System.Collections;
using GameObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

// ReSharper disable InconsistentNaming

namespace UI
{
    public class MenuUIManager : MonoBehaviour
    {
        #region Global

        public Image fade;
        public float fadeTime = 0.5f;

        [Header("Sound")] public SimpleSFX sceneChangeSound;
        public AudioSource musicAudio;

        private enum UIState
        {
            MainMenu,
            History,
            Settings
        }

        private UIState _state;
        private AudioSource _audioSource;
        private Settings _settings;

        public Settings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = GetComponent<Settings>();
                return _settings;
            }
        }

        private void Start()
        {
            versionText.text = "v" + Application.version;
            
            _audioSource = GetComponent<AudioSource>();

            UpdateUI();
            BindMainMenuButtons();
            BindHistoryMenuButtons();
            BindSettingsMenuButtons();
        }

        private void LateUpdate()
        {
            switch (_state)
            {
                case UIState.MainMenu:
                    if (InputManager.GetKeyDown("Menu"))
                    {
                        Quit();
                    }

                    break;
                case UIState.History:
                    if (InputManager.GetKeyDown("Menu") || InputManager.GetKeyDown("Cancel"))
                    {
                        MainUI();
                    }

                    break;
                case UIState.Settings:
                    if (!settingsMenu.locked && (InputManager.GetKeyDown("Menu") || InputManager.GetKeyDown("Cancel")))
                    {
                        MainUI();
                    }

                    break;
            }
        }

        private void UpdateUI()
        {
            mainMenu.SetActive(_state == UIState.MainMenu);
            historyMenu.SetActive(_state == UIState.History);
            settingsMenu.gameObject.SetActive(_state == UIState.Settings);
        }

        private IEnumerator FadeOut(float time, UnityAction finished)
        {
            fade.gameObject.SetActive(true);
            while (Mathf.Abs(1f - fade.color.a) > 0.1f)
            {
                GameUtils.SetFadeAlpha(fade, Color.black, 1f, time);
                yield return null;
            }

            finished();
        }

        private void Play()
        {
            var sound = Instantiate(sceneChangeSound, FindObjectOfType<Camera>().transform.position,
                Quaternion.identity);
            DontDestroyOnLoad(sound);
            StartCoroutine(GameUtils.FadeOutAudio(musicAudio, fadeTime));
            StartCoroutine(FadeOut(fadeTime, () => SceneManager.LoadScene("Game")));
        }

        private void Quit()
        {
            StartCoroutine(GameUtils.FadeOutAudio(musicAudio, fadeTime / 2));
            StartCoroutine(FadeOut(fadeTime / 2, Application.Quit));
        }

        #endregion

        #region Main Menu

        [Header("Main Menu")] public GameObject mainMenu;

        public TextMeshProUGUI versionText;
        
        public Button playButton;
        public Button historyButton;
        public Button settingsButton;
        public Button quitButton;

        private void BindMainMenuButtons()
        {
            playButton.onClick.AddListener(Play);
            historyButton.onClick.AddListener(HistoryUI);
            settingsButton.onClick.AddListener(SettingsUI);
            quitButton.onClick.AddListener(Quit);
        }

        private void MainUI()
        {
            _state = UIState.MainMenu;
            UpdateUI();
        }

        #endregion

        #region History Menu

        [Header("History Menu")] public GameObject historyMenu;
        public LeaderBoard leaderBoard;
        public Button backButton;

        private void BindHistoryMenuButtons()
        {
            backButton.onClick.AddListener(MainUI);
        }

        public void HistoryUI()
        {
            _state = UIState.History;
            UpdateUI();
            leaderBoard.Show();
        }

        #endregion

        #region Settings Menu

        [Header("Settings Menu")] public SettingsMenu settingsMenu;
        public Button backButton2;

        private void BindSettingsMenuButtons()
        {
            backButton2.onClick.AddListener(MainUI);
        }

        public void SettingsUI()
        {
            _state = UIState.Settings;
            UpdateUI();
        }

        #endregion
    }
}