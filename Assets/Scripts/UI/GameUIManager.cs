using System;
using System.Collections;
using GameObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
// ReSharper disable InconsistentNaming

namespace UI
{
    public class GameUIManager : MonoBehaviour
    {
        #region Global
        
        private enum UIState
        {
            Game,
            Pause,
            Settings,
            GameOver
        }

        private UIState _state;
        
        private GameManager _gameManager;

        public Image fade;
        public float fadeTime = 0.5f;
        
        [Header("Audio")]
        public AudioClip looseSound;

        public AudioSource musicAudio;
        private AudioSource _audioSource;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _audioSource = GetComponent<AudioSource>();
            UpdateUI();
            BindPauseMenuButtons();
            BindSettingsMenuButtons();
            BindGameOverMenuButtons();
        }
        
        private void LateUpdate()
        {
            switch (_state)
            {
                case UIState.Game:
                    if (InputManager.GetKeyDown("Menu"))
                    {
                        PauseAction();
                    }
                    break;
                case UIState.Settings:
                    if (!settingsMenu.locked && (InputManager.GetKeyDown("Menu") || InputManager.GetKeyDown("Cancel")))
                    {
                        PauseUI();
                    }
                    break;
                case UIState.Pause:
                    if (InputManager.GetKeyDown("Menu") || InputManager.GetKeyDown("Cancel"))
                    {
                        ResumeAction();
                    }
                    break;
            }
        }

        private void UpdateUI()
        {
            pauseMenu.SetActive(_state == UIState.Pause);
            settingsMenu.gameObject.SetActive(_state == UIState.Settings);
            gameOverMenu.SetActive(_state == UIState.GameOver);
        }

        private IEnumerator FadeOut(float time, UnityAction finished)
        {
            fade.gameObject.SetActive(true);
            fade.color = new Color(0,0,0,0);
            while (Mathf.Abs(1f-fade.color.a) > 0.1f) {
                GameUtils.SetFadeAlpha(fade, Color.black, 1f, time);
                yield return null;
            }

            finished();
        }
        
        #endregion

        #region HUD

        [Header("HUD")] 
        public GameObject hud;
        public StatusBar healthBar;
        public Score score;
        
        private void ResumeAction()
        {
            GameUI();
            _gameManager.Resume();
        }
        
        public void GameUI()
        {
            _state = UIState.Game;
            UpdateUI();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void UpdateHealth(int value, int max)
        {
            healthBar.UpdateValue(value, max);
        }

        public void UpdateHealth(int value)
        {
            healthBar.UpdateValue(value);
        }

        public void AddScore(int value)
        {
            score.AddPoints(value);
        }
        
        #endregion
        
        #region Pause Menu
        
        [Header("Pause Menu")] 
        public GameObject pauseMenu;

        public Button resumeButton;
        public Button settingsButton;
        public Button menuButton;

        private void BindPauseMenuButtons()
        {
            resumeButton.onClick.AddListener(ResumeAction);
            settingsButton.onClick.AddListener(SettingsUI);
            menuButton.onClick.AddListener((() =>
            {
                StartCoroutine(GameUtils.FadeOutAudio(musicAudio, fadeTime));
                StartCoroutine(FadeOut(fadeTime,_gameManager.Menu));
            }));
        }
        
        private void PauseAction()
        {
            PauseUI();
            _gameManager.Pause();
        }
        
        private void PauseUI()
        {
            _state = UIState.Pause;
            UpdateUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion
        
        #region Settings Menu

        [Header("Settings Menu")]
        public SettingsMenu settingsMenu;
        public Button backButton;
        
        private void BindSettingsMenuButtons()
        {
            backButton.onClick.AddListener(PauseUI);
        }

        private void SettingsUI()
        {
            _state = UIState.Settings;
            UpdateUI();
        }
        
        #endregion
        
        #region Game Over Menu
        
        [Header("Game Over Menu")]
        public GameObject gameOverMenu;
        public LeaderBoard leaderBoard;
        public TextMeshProUGUI finalScoreText;
        public Button restartButton;
        public Button menuButton2;
        
        private void BindGameOverMenuButtons()
        {
            restartButton.onClick.AddListener(_gameManager.Restart);
            menuButton2.onClick.AddListener(() =>
            {
                StartCoroutine(FadeOut(fadeTime, _gameManager.Menu));
            });
        }
        
        public void GameOverUI(int finalScore)
        {
            StartCoroutine(GameUtils.FadeOutAudio(musicAudio, fadeTime/2f));
            StartCoroutine(FadeOut(fadeTime/2f, () =>
            {
                hud.SetActive(false);
                fade.gameObject.SetActive(false);
                _audioSource.PlayOneShot(looseSound);
                _state = UIState.GameOver;
                UpdateUI();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                leaderBoard.AddScoreAndShow(finalScore);
                finalScoreText.text = finalScore.ToString();
            }));
            
        }
        
        #endregion
        
    }
}
