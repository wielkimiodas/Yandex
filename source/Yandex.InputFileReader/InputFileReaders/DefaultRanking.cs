using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    public class DefaultRanking : InputFileReader
    {
        private int _sessionId;
        private StreamWriter _writer;
        private readonly string _output;
        private Random _random;
        private readonly Tuple<int, int>[] _swapTypes;
        private const int TestQuery = 2;

        public DefaultRanking(string output)
        {
            _output = output;
            _random = new Random();
            _swapTypes = new[]
            {
                //insert here value - 1
                new Tuple<int, int>(1, 2),
                //new Tuple<int, int>(2, 3),
                //new Tuple<int, int>(6, 7);
            };
        }

        public override void onBeginRead()
        {
            _writer = new StreamWriter(_output);
            _writer.WriteLine("SessionID,URLID");
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            if (queryAction.type == TestQuery)
            {
                //swap for 50% of data
                if (_random.Next(100) < 50)
                {
                    var type = _swapTypes[_random.Next(_swapTypes.Length)];
                    int p = queryAction.urls[type.Item1];
                    queryAction.urls[type.Item1] = queryAction.urls[type.Item2];
                    queryAction.urls[type.Item2] = p;
                }
                for (int i = 0; i < queryAction.nUrls - 1; i++)
                {
                    _writer.WriteLine(_sessionId + "," + queryAction.urls[i]);
                }
            }
        }

        public override void onMetadata(Metadata metadata)
        {
            _sessionId = metadata.sessionId;
        }

        public override void onEndRead()
        {
            _writer.Flush();
            _writer.Close();
        }
    }
}
