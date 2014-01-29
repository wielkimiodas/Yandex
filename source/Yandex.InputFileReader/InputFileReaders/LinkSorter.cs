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
        //private StreamWriter _relevantWriter;
        //private StreamWriter _veryRelevantWriter;

        private int _currentUrlClicked = -1;

        private int _lastDwellTime = -1;
        private int _currentDwellTime=-1;
        private bool _isLastActionClick;

        private List<int> _relevants;
        private List<int> _veryRelevants;
        private List<int> _occurences;
        private List<Tuple<int, float>> _result;

        private const int UrlMaxId = 71224114 + 1;

        public override void onBeginRead()
        {
            //_relevantWriter = new StreamWriter(PathResolver.RelevantUrlsFile);
            //_veryRelevantWriter = new StreamWriter(PathResolver.VeryRelevantUrlsFile);

            _relevants = new List<int>();
            _veryRelevants = new List<int>();
            _occurences = new List<int>();
            _result = new List<Tuple<int, float>>();

            for (int i = 0; i < UrlMaxId; i++)
            {
                _occurences.Add(0);
                _relevants.Add(0);
                _veryRelevants.Add(0);
            }
        }

        public override void onClick(Click click)
        {
            _currentUrlClicked = click.urlId;

            _lastDwellTime = _currentDwellTime;
            _currentDwellTime = click.time;

            int diff = _currentDwellTime - _lastDwellTime;
            if (diff <= 399 && diff >= 50)
            {
                //_relevantWriter.WriteLine(click.urlId);
                _relevants[click.urlId]++;
            }
            else if (diff >= 400)
            {
                //_veryRelevantWriter.WriteLine(click.urlId);
                _veryRelevants[click.urlId]++;
            }
            _isLastActionClick = true;
        }

        public override void onMetadata(Metadata metadata)
        {
            if (_isLastActionClick)
            {
                //_veryRelevantWriter.WriteLine(_currentUrlClicked);
                _veryRelevants[_currentUrlClicked]++;
            }

            _lastDwellTime = -1;
            _currentDwellTime = -1;
            _currentUrlClicked = -1;
            _isLastActionClick = false;
        }
        
        public override void onQueryAction(QueryAction queryAction)
        {
            _isLastActionClick = false;

            for (int i = 0; i < queryAction.nUrls;i++)
            {
                _occurences[queryAction.urls[i]]++;
            }
        }

        public override void onEndRead()
        {
            if (_isLastActionClick)
            {
                //_veryRelevantWriter.WriteLine(_currentUrlClicked);
                _veryRelevants[_currentUrlClicked]++;
            }

            AnalyzeResults();
            SaveToFile();

            //_veryRelevantWriter.Flush();
            //_relevantWriter.Flush();
            //_relevantWriter.Close();
            //_veryRelevantWriter.Close();
            
        }

        private void AnalyzeResults()
        {
            Console.WriteLine("Computing results...");
            for (int i = 0; i < UrlMaxId; i++)
            {
                float res = (_veryRelevants[i]*2 + _relevants[i])/(float)_occurences[i];
                if (res > 0)
                {
                    _result.Add(new Tuple<int, float>(i,res));
                }
            }

            Console.WriteLine("Sorting...");
            _result.Sort((x1,x2)=>x2.Item2.CompareTo(x1.Item2));
        }

        private void SaveToFile()
        {
            Console.WriteLine("Saving to file "+_result.Count+" urls...");
            
            var resultWriter = new StreamWriter(PathResolver.ClicksAnalyse);
            for (int i = 0; i < _result.Count; i++)
            {
                resultWriter.WriteLine(_result[i].Item1 + "\t" +_result[i].Item2);
            }

            resultWriter.Flush();
            resultWriter.Close();
        }
    }
}
