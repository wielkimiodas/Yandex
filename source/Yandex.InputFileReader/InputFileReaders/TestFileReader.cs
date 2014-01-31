using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    public class UsersGroup
    {
        public BinarySearchSet<int> users { get; private set; }
        public List<Tuple<int, float>> urlsStats { get; private set; }

        public UsersGroup(BinarySearchSet<int> users, List<Tuple<int, float>> urlsStats)
        {
            this.users = users;
            this.urlsStats = urlsStats;
        }
    }

    public class TestFileReader : InputFileReader
    {
        private readonly BinarySearchSet<int> _users;
        private readonly BinaryWriter _writer;
        private bool _isUserInGroup;
        private readonly List<UsersGroup> _statistics;
        public TestFileReader(BinaryWriter writer, List<UsersGroup> statistics)
        {
            _writer = writer; 
            _statistics = statistics;
        }

        public override void onMetadata(Metadata metadata)
        {
            //determines if we have statistics for user from the session
            _isUserInGroup = _users.Contains(metadata.userId);
            metadata.WriteToStream(_writer);
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            //have statistics for current user
            if (_isUserInGroup)
            {
                //if it is T type query which we are requested to rearrange
                if (queryAction.type == 2)
                {
                    float defFactor = 1;

                    for (int i = 0; i < queryAction.nUrls; i++)
                    {
                        
                        //generally - factors from 1 to 10 inclusively
                        float currentQueryFactor = queryAction.nUrls - i;
                        float rankingFactor = 
                    }
                    
                }
            }
            else
            {
                queryAction.WriteToStream(_writer);
            }
        }

        public override void onClick(Click click)
        {
            click.WriteToStream(_writer);
        }
    }
}
