using System;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils
{
    public static class PlayerData
    {
        private const string FileName = "player.config";
        private static KeyValueFile _file;

        private static KeyValueFile File => _file ?? (_file = new KeyValueFile(Application.persistentDataPath + Path.DirectorySeparatorChar + FileName));

        public static bool HasKey(string key)
        {
            return File.HasKey(key);
        }

        public static void Reset(string key)
        {
            File.Remove(key);
        }
        
        public static string GetString(string key)
        {
            return File.Get(key);
        }
        
        public static void SetString(string key, string value)
        {
            File.Set(key, value);
        }

        public static KeyCode GetKeyCode(string key, KeyCode defaultValue)
        {
            return File.HasKey(key) && Enum.TryParse<KeyCode>(File.Get(key), out var value) ? value : defaultValue;
        }
        
        public static T GetEnumValue<T>(string key, T defaultValue) where T : struct
        {
            return File.HasKey(key) && Enum.TryParse<T>(File.Get(key), out var value) ? value : defaultValue;
        }
        
        public static void SetEnumValue<T>(string key, T value) where T : struct, Enum
        {
            File.Set(key, value.ToString());
        }
        
        public static void SetKeyCode(string key, KeyCode value)
        {
            File.Set(key, value.ToString());
        }

        public static int GetInt(string key, int defaultValue)
        {
            return File.HasKey(key) && int.TryParse(File.Get(key), out var value) ? value : defaultValue;
        }
        
        public static void SetInt(string key, int value)
        {
            File.Set(key, value.ToString());
        }

        public static float GetFloat(string key, float defaultValue)
        {
            return File.HasKey(key) && float.TryParse(File.Get(key), out var value) ? value : defaultValue;
        }
        
        public static void SetFloat(string key, float value)
        {
            File.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void Reload()
        {
            File.Reload();
        }
        
        public static void Save()
        {
            File.Save();
        }
    }
}