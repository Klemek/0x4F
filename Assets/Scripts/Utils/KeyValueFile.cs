using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public class KeyValueFile
    {
        private string Path { get; }

        private Dictionary<string, string> _content;

        private Dictionary<string, string> Content
        {
            get
            {
                if (_content == null)
                {
                    Reload();
                }
                return _content;
            }
        }

        public KeyValueFile(string path)
        {
            Path = path;
        }

        public void Reload()
        {
            _content = new Dictionary<string, string>();
            if (!File.Exists(Path)) return;
            try
            {
                foreach (var line in File.ReadAllLines(Path))
                {
                    var spl = line.Split("=".ToCharArray(), 2);
                    if (spl.Length == 2)
                    {
                        _content[spl[0].Trim()] = spl[1].Trim();
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }

        public void Save()
        {
            try
            {
                if (!File.Exists(Path)) File.Create(Path);
                var lines = Content.Keys.Select(k => k.Trim() + "=" + Content[k].Trim());
                File.WriteAllLines(Path, lines);
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }

        public bool HasKey(string key)
        {
            return Content.ContainsKey(key);
        }
    
        public string Get(string key)
        {
            return Content[key];
        }

        public void Set(string key, string value)
        {
            Content[key] = value;
        }

        public void Remove(string key)
        {
            Content.Remove(key);
        }
    }
}