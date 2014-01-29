using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    /// <summary>
    /// Na podstawie zdarzen, osądza które urle (domeny) są bardzo ważne, ważne, a które nie warte uwagi dla użytkownika.
    /// </summary>
    public class LinkSorter : InputFileReader
    {
        private int _currentUrlClicked = -1;

        private int _lastDwellTime = -1;
        private int _currentDwellTime = -1;
        private bool _isLastActionClick;

        private int[][] _relevants;
        private int[][] _veryRelevants;
        private int[][] _occurences;
        private readonly StreamWriter _writer;
        private List<List<Tuple<int, float>>> _results;
        private readonly List<BinarySearchSet<int>> _userGroupsList;
        private int groupId;

        private const int UrlMaxId = 71224114 + 1;

        public LinkSorter(List<BinarySearchSet<int>> userGroupsList, StreamWriter writer)
        {
            _userGroupsList = userGroupsList;
            _writer = writer;
        }

        public override void onBeginRead()
        {
            _relevants = new int[_userGroupsList.Count][];
            for (int i = 0; i < _relevants.Length; i++)
                _relevants[i] = new int[UrlMaxId];
            _veryRelevants = new int[_userGroupsList.Count][];
            for (int i = 0; i < _veryRelevants.Length; i++)
                _veryRelevants[i] = new int[UrlMaxId];
            _occurences = new int[_userGroupsList.Count][];
            for (int i = 0; i < _occurences.Length; i++)
                _occurences[i] = new int[UrlMaxId];
            _results = new List<List<Tuple<int, float>>>();
        }

        public override void onClick(Click click)
        {
            if (groupId == -1) return;

            _isLastActionClick = true;
            _currentUrlClicked = click.urlId;

            _lastDwellTime = _currentDwellTime;
            _currentDwellTime = click.time;

            int diff = _currentDwellTime - _lastDwellTime;
            if (diff <= 399 && diff >= 50)
            {
                _relevants[groupId][click.urlId]++;
                //nie chcemy goscia liczyc dwa razy
                _isLastActionClick = false;
            }
            else if (diff >= 400)
            {
                _veryRelevants[groupId][click.urlId]++;
                //nie chcemy goscia liczyc dwa razy
                _isLastActionClick = false;
            }
        }

        public override void onMetadata(Metadata metadata)
        {
            groupId = -1;
            for (int i = 0; i < _userGroupsList.Count; i++)
            {
                if (_userGroupsList[i].Contains(metadata.userId))
                {
                    groupId = i;
                    break;
                }
            }

            if (groupId == -1) return;

            if (_isLastActionClick)
            {

                _veryRelevants[groupId][_currentUrlClicked]++;
            }

            _lastDwellTime = -1;
            _currentDwellTime = -1;
            _currentUrlClicked = -1;
            _isLastActionClick = false;
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            if (groupId == -1) return;

            _isLastActionClick = false;
            _currentDwellTime = queryAction.time;
            for (int i = 0; i < queryAction.nUrls; i++)
            {
                _occurences[groupId][queryAction.urls[i]]++;
            }
        }

        public override void onEndRead()
        {
            if (_isLastActionClick && groupId > -1)
            {
                _veryRelevants[groupId][_currentUrlClicked]++;
            }

            AnalyzeResults();
            SaveToFile();
        }

        private void AnalyzeResults()
        {
            Console.WriteLine("Computing results...");

            for (int j = 0; j < _userGroupsList.Count; j++)
            {
                for (int i = 0; i < UrlMaxId; i++)
                {
                    float res = (_veryRelevants[j][i] * 2 + _relevants[j][i]) / (float)_occurences[j][i];
                    if (res > 0)
                    {
                        _results[j].Add(new Tuple<int, float>(i, res));
                    }
                }

                Console.WriteLine("Sorting...");
                _results[j].Sort((x1, x2) => x2.Item2.CompareTo(x1.Item2));
            }
        }

        private void SaveToFile()
        {
            Console.WriteLine("Saving to file...");

            for (int j = 0; j < _userGroupsList.Count; j++)
            {
                for (int i = 0; i < _userGroupsList[j].Count; i++)
                {
                    _writer.WriteLine(_userGroupsList[j].ElementAt(i));
                }

                _writer.WriteLine();

                for (int i = 0; i < _results[j].Count; i++)
                {
                    var res = _results[j][i];
                    _writer.WriteLine(res.Item1 + "\t" + res.Item2);
                }

                _writer.WriteLine();
            }
            

            _writer.Flush();
            _writer.Close();
        }
    }
}
