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
        public BinarySearchMultiSet<Tuple<int, float>> urlsStats { get; private set; }

        public UsersGroup(BinarySearchSet<int> users, BinarySearchMultiSet<Tuple<int, float>> urlsStats)
        {
            this.users = users;
            this.urlsStats = urlsStats;
        }
    }

    public class TestFileReader : InputFileReader
    {
        private readonly BinaryWriter _writer;
        private int _userGroupId = -1;
        private readonly List<UsersGroup> _statistics;
        public TestFileReader(BinaryWriter writer, List<UsersGroup> statistics)
        {
            _writer = writer;
            _statistics = statistics;
        }

        public override void onMetadata(Metadata metadata)
        {
            _userGroupId = -1;
            ParallelWorker worker = new ParallelWorker(8);
            bool found = false;
            for (int it = 0; it < _statistics.Count && !found; it++)
            {
                int i = it;
                worker.Queue(delegate
                {
                    if (_statistics[i].users.Contains(metadata.userId))
                    {
                        _userGroupId = i;
                        found = true;
                    }
                });
            }
            metadata.WriteToStream(_writer);
            worker.Wait();
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            //have statistics for current user
            if (_userGroupId > 0)
            {
                //if it is T type query which we are requested to rearrange
                if (queryAction.type == 2)
                {
                    var ourFinalRanking = new List<Tuple<int, float>>();
                    const float defFactor = 1;
                    const float statisticsFactor = 1;

                    for (int i = 0; i < queryAction.nUrls; i++)
                    {
                        float statisticsValue = -1;
                        //generally - factors from 1 to 10 inclusively
                        float ourRank = (queryAction.nUrls - i) * defFactor;

                        var currUserGroup = _statistics[_userGroupId];
                        int indexOf = _statistics[_userGroupId].urlsStats.IndexOf(new Tuple<int,float>(queryAction.urls[i], 0.0f));
                        if (indexOf >= 0)
                        {
                            var elem = currUserGroup.urlsStats.ElementAt(indexOf);
                            statisticsValue = elem.Item2;
                        }
                        /*int kTresh = _statistics[_userGroupId].urlsStats.Count;
                        for (int k = 0; k < kTresh; k++)
                        {
                            var elem = currUserGroup.urlsStats.ElementAt(k);
                            if (elem.Item1 == queryAction.urls[i])
                            {
                                statisticsValue = elem.Item2;
                                break;
                            }
                        }*/
                        
                        if (statisticsValue > 0)
                        {
                            ourRank += statisticsValue * statisticsFactor;
                        }
                        ourFinalRanking.Add(new Tuple<int, float>(queryAction.urls[i], ourRank));
                    }
                    ourFinalRanking.Sort((x1, x2) => x2.Item2.CompareTo(x1.Item2));

                    for (int i = 0; i < queryAction.nUrls; i++)
                    {
                        queryAction.urls[i] = ourFinalRanking.ElementAt(i).Item1;
                    }
                }
            }
            queryAction.WriteToStream(_writer);
        }

        public override void onClick(Click click)
        {
            click.WriteToStream(_writer);
        }
    }
}
