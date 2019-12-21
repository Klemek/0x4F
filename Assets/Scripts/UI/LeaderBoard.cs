using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class LeaderBoard : MonoBehaviour
    {
        public TextMeshProUGUI entryTextTemplate;
        public int lineCount;
        public Color lastColor;

        private TextMeshProUGUI[] _textLines;
        private List<Tuple<DateTime, int>> _list;

        private List<Tuple<DateTime, int>> List
        {
            get
            {
                if(_list == null)
                    LoadLeaderBoard();
                return _list;
            }
        }

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            _textLines = new TextMeshProUGUI[lineCount];
            var height = entryTextTemplate.rectTransform.sizeDelta.y;
            for (var i = 0; i < lineCount; i++)
            {
                _textLines[i] = Instantiate(entryTextTemplate, rectTransform);
                _textLines[i].rectTransform.pivot = entryTextTemplate.rectTransform.pivot;
                _textLines[i].rectTransform.localPosition = entryTextTemplate.rectTransform.localPosition;
                _textLines[i].text = "---";
                _textLines[i].rectTransform.localPosition -= new Vector3(0, i * height, 0);
            }
            Destroy(entryTextTemplate.gameObject);
        }

        private void LoadLeaderBoard()
        {
            _list = new List<Tuple<DateTime, int>>();
            if (!PlayerData.HasKey("LeaderBoard")) return;
            var data = PlayerData.GetString("LeaderBoard").Split(';').Select(e => e.Split('|'));
            foreach (var entry in data)
            {
                if (entry.Length == 2 && DateTime.TryParse(entry[0], out var time) && int.TryParse(entry[1], out var score))
                {
                    _list.Add(new Tuple<DateTime, int>(time, score));
                }
            }
        }
        
        private void SaveLeaderBoard()
        {
            var data = List.Select(e => e.Item1.ToString("O") + "|" + e.Item2)
                .Aggregate((e1, e2) => e1 + ";" + e2);
            PlayerData.SetString("LeaderBoard", data);
            PlayerData.Save();
        }

        public void AddScoreAndShow(int score)
        {
            var now = DateTime.Now;
            List.Add(new Tuple<DateTime, int>(now, score));
            List.Sort((s1, s2) => s2.Item2.CompareTo(s1.Item2));
            SaveLeaderBoard();
            Show(now);
        }

        public void Show()
        {
            Show(DateTime.Now);
        }

        public void Show(DateTime last)
        {
            for (var i = 0; i < Math.Min(lineCount, List.Count); i++)
            {
                _textLines[i].text = List[i].Item1.ToString("dd/MM/yy hh:mm") + "  -  " + List[i].Item2;
                if (List[i].Item1 == last)
                {
                    _textLines[i].color = lastColor;
                }

                if (i == 0 || List[i].Item1 == last)
                {
                    _textLines[i].fontStyle = FontStyles.Bold;
                }
            }
        }
    }
}