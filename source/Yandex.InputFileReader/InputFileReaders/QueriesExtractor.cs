using System.Collections.Generic;
using System.IO;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class QueriesExtractor : InputFileReader
    {
        private string output;
        private HashSet<int> processedQueries = new HashSet<int>();
        private BinaryWriter writer = null;

        public QueriesExtractor(string output)
        {
            this.output = output;
        }

        public override void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }

        public override void onBeginRead()
        {
            writer = new BinaryWriter(new FileStream(output, FileMode.CreateNew));
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            bool process = !processedQueries.Contains(queryAction.queryId);
            if (process)
                processedQueries.Add(queryAction.queryId);

            if (!process)
                return;

            writer.Write(queryAction.queryId);
            writer.Write(queryAction.sessionId);
            writer.Write(queryAction.serpid);

            writer.Write(queryAction.nTerms);
            for (int i = queryAction.nTerms - 1; i >= 0; i--)
                writer.Write(queryAction.terms[i]);

            writer.Write(queryAction.nUrls);
            for (int i = queryAction.nUrls - 1; i >= 0; i--)
            {
                writer.Write(queryAction.urls[i]);
                writer.Write(queryAction.domains[i]);
            }
        }

        public override void onEndRead()
        {
            writer.Close();
            writer.Dispose();
            writer = null;
        }
    }
}