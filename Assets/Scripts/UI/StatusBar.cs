using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteInEditMode]
    public class StatusBar : MonoBehaviour
    {
        private RectTransform _rect;

        [Header("Values")] public int value;
        public int max;
        public Color color;

        [Header("Components")] public Image background;
        public Image foreground;
        public TextMeshProUGUI text;

        [Header("Change")] public float statusUpdateTime = .5f;

        public void Start()
        {
            SetUp();
        }

        private void SetUp()
        {
            _rect = GetComponent<RectTransform>();
            if (background)
                background.color = color;
            if (foreground)
                foreground.color = color;
        }

        public void UpdateValue(int newValue)
        {
            UpdateValue(newValue, max);
        }

        public void UpdateValue(int newValue, int newMax)
        {
            value = newValue;
            max = newMax;
            Refresh();
        }

        public void Refresh()
        {
            if (foreground)
            {
                StopCoroutine(nameof(ChangeSmooth));
                StartCoroutine(nameof(ChangeSmooth));
            }
            if (text)
                text.text = value + " / " + max;
        }

        private IEnumerator ChangeSmooth()
        {
            var current = foreground.rectTransform.sizeDelta.x;
            var target = _rect.sizeDelta.x * Mathf.Min(value / (float) max);
            while (Mathf.Abs(current-target) > 0.1f)
            {
                current = Mathf.Lerp(current, target, Time.unscaledDeltaTime / statusUpdateTime);
                foreground.rectTransform.sizeDelta = new Vector2(current, _rect.sizeDelta.y);
                yield return null;
            }
            FreezeStatus();
        }

        public void FreezeStatus()
        {
            StopCoroutine(nameof(ChangeSmooth));
            foreground.rectTransform.sizeDelta =_rect.sizeDelta * new Vector2(Mathf.Min(value / (float) max), 1f);
        }

        private void OnValidate()
        {
            SetUp();
            Refresh();
            FreezeStatus();
        }
    }
}