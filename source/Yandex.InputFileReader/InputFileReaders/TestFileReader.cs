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
        private bool _isUserInGroup;
        private readonly List<Tuple<int, float>> _statistics;
        public TestFileReader(BinaryWriter writer, List<int> users, List<Tuple<int,float>> statistics)
        {
            _writer = writer; 
            _users = users;
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
                    //explore statistics
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
