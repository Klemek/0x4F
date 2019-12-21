using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class InputNotification : MonoBehaviour
    {
        public Image joystick;
        public Image keyboard;

        private const float NotificationTime = .5f;
        private const float FadeTime = .25f;
    
        private Image _notification;
        private bool _connected;

        private void Start()
        {
            InputManager.AddJoystickListener(OnJoystickChange);
            _notification = GetComponent<Image>();
            Reset();
            gameObject.SetActive(false);
        }

        private void Reset()
        {
            _notification.color = new Color(1, 1, 1, 0);
            joystick.color = new Color(1, 1, 1, 0);
            keyboard.color = new Color(1, 1, 1, 0);
        }

        private void OnJoystickChange(bool connected)
        {
            _connected = connected;
            Reset();
            gameObject.SetActive(true);
            StopCoroutine(nameof(ShowNotification));
            StartCoroutine(nameof(ShowNotification));
        }

        private IEnumerator ShowNotification()
        {
            while (.5f - _notification.color.a > 0.0001f)
            {
                GameUtils.SetFadeAlphaWhite(_notification, .5f, FadeTime);
                GameUtils.SetFadeAlphaWhite(_connected ? joystick : keyboard, 1f, FadeTime);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(NotificationTime);
            while (_notification.color.a > 0.0001f)
            {
                GameUtils.SetFadeAlphaWhite(_notification, 0f, FadeTime);
                GameUtils.SetFadeAlphaWhite(_connected ? joystick : keyboard, 0f, FadeTime);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}