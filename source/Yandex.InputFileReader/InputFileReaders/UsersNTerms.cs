using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class UsersNTerms : InputFileReader
    {
        private List<BinarySearchSet<int>> usersTerms = null;
        private BinarySearchSet<int> currentList;
        private string outputFile;

        public UsersNTerms(String outputFile)
        {
            this.outputFile = outputFile;
        }

        public override void onBeginRead()
        {
            usersTerms = new List<BinarySearchSet<int>>();
            currentList = null;
        }

        public BinarySearchSet<int> getList(int userId)
        {
            while (usersTerms.Count < userId)
                usersTerms.Add(null);

            if (usersTerms.Count == userId)
                usersTerms.Add(new BinarySearchSet<int>(Comparer<int>.Default));

            BinarySearchSet<int> list = usersTerms[userId];
            if (list == null)
            {
                list = new BinarySearchSet<int>(Comparer<int>.Default);
                usersTerms[userId] = list;
            }

            return list;
        }

        public override void onMetadata(Metadata metadata)
        {
            currentList = getList(metadata.userId);
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            for (int i = queryAction.nTerms - 1; i >= 0; i--)
                currentList.Add(queryAction.terms[i]);
        }

        public override void onEndRead()
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(outputFile, FileMode.CreateNew)))
            {
                for (int i = 0; i < usersTerms.Count; i++)
                {
                    var list = usersTerms[i];
                    if (list == null)
                        continue;
                    usersTerms[i] = null;
                    writer.Write((int)i);
                    writer.Write(list.Count);
                    foreach (int value in list)
                        writer.Write(value);
                }
            }
        }
    }
}