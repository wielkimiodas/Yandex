using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    public class TestFileReader : InputFileReader
    {
        private readonly List<int> _users;
        private readonly BinaryWriter _writer;
        private bool isUserInGroup;
        private readonly List<Tuple<int, float>> _statistics;
        public TestFileReader(BinaryWriter writer, List<int> users, List<Tuple<int,float>> statistics)
        {
            _writer = writer; 
            _users = users;
            _statistics = statistics;
        }

        public override void onMetadata(Metadata metadata)
        {
            isUserInGroup = _users.Contains(metadata.userId);

            metadata.WriteToFile(_writer);
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            List<Tuple<int, float,int>> blub = null;
            if (queryAction.type == 2)
            {
                blub = new List<Tuple<int, float,int>>();
                for (int i = 0; i < queryAction.nUrls; i++)
                {
                    var res = _statistics.Find(x => x.Item1 == queryAction.urls[i]);
                    if (res != null) blub.Add(new Tuple<int, float,int>(res.Item1,res.Item2,i));
                }
            }

            if (blub != null)
            {
                blub.Sort((x,y)=>y.Item2.CompareTo(x.Item2));
                foreach (var tuple in blub)
                {
                    
                }
            }

        }

        public override void onClick(Click click)
        {
            click.WriteToFile(_writer);
        }
    }
}
