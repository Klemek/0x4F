using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class GameUtils
    {
        public static string Prettify(string src)
        {
            return Regex.Replace(src, "([A-Z0-9])", " $1").Trim();
        }

        // ReSharper disable once InconsistentNaming
        public static void PlayRandomSFX(AudioSource source, float pitchRange, IReadOnlyList<AudioClip> list)
        {
            if (list.Count <= 0) return;
            PlayRandomSFX(source, pitchRange, list[Random.Range(0, list.Count)]);
        }

        public static void PlayRandomSFX(AudioSource source, float pitchRange, AudioClip clip)
        {
            source.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
            source.PlayOneShot(clip);
        }

        public static float VolumeToDecibels(int v)
        {
            if (v == 0)
                return -80f;
            return 20 * Mathf.Log10(v / 100f);
        }

        public static IEnumerator FadeOutAudio(AudioSource audioSource, float fadeTime)
        {
            var startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.unscaledDeltaTime / fadeTime;
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        public static void SetFadeAlphaWhite(Graphic src, float target, float fadeTime)
        {
            SetFadeAlpha(src, Color.white, target, fadeTime);
        }

        public static void SetFadeAlpha(Graphic src, Color baseColor, float target, float fadeTime)
        {
            src.color = new Color(baseColor.r, baseColor.g, baseColor.b,
                Mathf.Lerp(src.color.a, target, Time.unscaledDeltaTime / fadeTime));
        }

        public static int GetCurrentResolution()
        {
            var width = Screen.fullScreen ? Screen.currentResolution.width : Screen.width;
            var height =  Screen.fullScreen ? Screen.currentResolution.height : Screen.height;
            var found = CommonResolutions.FirstOrDefault(r => r[0] == width && r[1] == height);
            return found == null ? 0 : Array.IndexOf(CommonResolutions, found);
        }
        
        public static string GetResolutionName(int[] res)
        {
            if (res[0] > 0)
                return res[0] + "×" + res[1];
            return "Custom";
        }

        public static readonly int[][] CommonResolutions =
        {
            new[] {0, 0},
            new[] {640, 480},
            new[] {800, 600},
            new[] {960, 720},
            new[] {1024, 576},
            new[] {1024, 768},
            new[] {1152, 648},
            new[] {1280, 720},
            new[] {1280, 800},
            new[] {1280, 960},
            new[] {1366, 768},
            new[] {1400, 1050},
            new[] {1440, 900},
            new[] {1440, 1080},
            new[] {1600, 900},
            new[] {1600, 1200},
            new[] {1680, 1050},
            new[] {1856, 1392},
            new[] {1920, 1080},
            new[] {1920, 1200},
            new[] {1920, 1440},
            new[] {2048, 1536},
            new[] {2560, 1440},
            new[] {2560, 1600},
            new[] {3840, 2160},
            new[] {7680, 4320}
        };
    }
}