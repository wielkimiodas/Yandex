using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class TopTermsGetter : InputFileReader
    {
        private readonly string _output;
        private readonly Dictionary<int, int> _termsCount = new Dictionary<int, int>();
        private readonly HashSet<int> _processedQueries = new HashSet<int>();
        private readonly HashSet<int> _currentTerms = new HashSet<int>();

        public TopTermsGetter(string output)
        {
            _output = output;
        }

        public override void Dispose()
        {
            _currentTerms.Clear();
            _termsCount.Clear();
            _processedQueries.Clear();
            GC.Collect();
        }

        public override void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            reader.ReadInt32();
            // QUERYID
            int queryId = reader.ReadInt32();

            bool process = !_processedQueries.Contains(queryId);
            if (process)
                _processedQueries.Add(queryId);

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // TERM ID
                int term = reader.ReadInt32();

                if (process)
                    _currentTerms.Add(term);
            }

            foreach (int term in _currentTerms)
                if (_termsCount.ContainsKey(term))
                    _termsCount[term]++;
                else
                    _termsCount.Add(term, 1);

            _currentTerms.Clear();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();
            }
        }

        public override void onEndRead()
        {
            var allTerms = _termsCount.Select(element => new Tuple<int, int>(element.Key, element.Value)).ToList();

            _termsCount.Clear();
            GC.Collect();

            allTerms.Sort((o1, o2) => o2.Item2 - o1.Item2);

            if (allTerms.Count > 1000)
                allTerms.RemoveRange(1000, allTerms.Count - 1000);

            using (var writer = new StreamWriter(_output))
            {
                foreach (var element in allTerms)
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
            }
        }
    }
}