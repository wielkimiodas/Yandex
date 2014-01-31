using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils;

namespace Yandex.LogProcessor
{
    public class FinalProcessor
    {
        
        private readonly StreamReader _reader;

        public FinalProcessor()
        {
            _reader = new StreamReader(PathResolver.ClicksAnalyse);
        }

        public void ReadTestInput()
        {
            Tuple<List<int>, List<Tuple<int, float>>> res = null;
            while ((res = ReadClickAnalyzerGroup()) != null)
            {
                var ms1 = new MemoryStream();
                using (var file = new FileStream(PathResolver.TestProcessedFile, FileMode.Open, FileAccess.Read))
                {
                    var bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int) file.Length);
                    ms1.Write(bytes, 0, (int) file.Length);
                }
                //how to do this with memory stream? 
                //var opener = new InputFileOpener(ms1, new TestFileReader(ms1,res.Item1,res.Item2));
            }
        }

        public Tuple<List<int>,List<Tuple<int,float>>> ReadClickAnalyzerGroup()
        {
            var statistics = new List<Tuple<int, float>>();
            var users = new List<int>();

            if (_reader.Peek() == -1) return null;
            
            string line;
            while (string.IsNullOrEmpty(line = _reader.ReadLine()))
            {
                //reading users 
                users.Add(Convert.ToInt32(line));
            }

            while (string.IsNullOrEmpty(line = _reader.ReadLine()))
            {
                //reading urls with correlated statistic
                var res = line.Split('\t');
                float val;
                float.TryParse(res[1], out val);
                statistics.Add(new Tuple<int, float>(Convert.ToInt32(res[0]),val));
            }

            //returns users and stats
            return new Tuple<List<int>, List<Tuple<int, float>>>(users,statistics);
        }
    }
}
