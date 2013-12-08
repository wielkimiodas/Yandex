using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class QueriesExtractor : InputFileReader
    {
        string output;
        HashSet<int> processedQueries = new HashSet<int>();
        BinaryWriter writer = null;

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

        public override void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            int sessionId = reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            int serpid = reader.ReadInt32();
            // QUERYID
            int queryId = reader.ReadInt32();

            bool process = !processedQueries.Contains(queryId);
            if (process)
                processedQueries.Add(queryId);

            if (!process)
            {
                for (int i = reader.ReadInt32(); i > 0; i--)
                    reader.ReadInt32();

                for (int i = reader.ReadInt32(); i > 0; i--)
                {
                    reader.ReadInt32();
                    reader.ReadInt32();
                }
            }
            else
            {
                writer.Write(queryId);
                writer.Write(sessionId);
                writer.Write(serpid);

                int nTerms = reader.ReadInt32();
                writer.Write(nTerms);
                for (int i = nTerms; i > 0; i--)
                    writer.Write(reader.ReadInt32());

                int nUrls = reader.ReadInt32();
                for (int i = nUrls; i > 0; i--)
                {
                    writer.Write(reader.ReadInt32());
                    writer.Write(reader.ReadInt32());
                }
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
