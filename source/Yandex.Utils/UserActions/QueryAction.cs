using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class QueryAction : UserAction
    {
        private static char[] commaSep = new char[] {','};

        public byte type { get; protected set; }
        public int sessionId { get; protected set; }
        public int time { get; protected set; }
        public int serpId { get; protected set; }
        public int queryId { get; protected set; }
        public int[] terms { get; protected set; }
        public int[] urls { get; protected set; }
        public int[] domains { get; protected set; }

        public QueryAction() { }

        public QueryAction(byte type, int sessionId, int time, int serpId, int queryId, int[] terms, int[] urls, int[] domains)
        {
            this.type = type;
            this.sessionId = sessionId;
            this.time = time;
            this.serpId = serpId;
            this.queryId = queryId;
            this.terms = terms;
            this.urls = urls;
            this.domains = domains;
        }

        public override bool readData(string[] array)
        {
            try
            {
                type = (byte) (array[2][0] == 'Q' ? 1 : 2);
                sessionId = Int32.Parse(array[0]);
                time = Int32.Parse(array[1]);
                serpId = Int32.Parse(array[3]);
                queryId = Int32.Parse(array[4]);

                string[] termsArray = array[5].Split(commaSep);
                terms = new int[termsArray.Length];
                for (int i = 0; i < termsArray.Length; i++)
                    terms[i] = Int32.Parse(termsArray[i]);

                urls = new int[array.Length - 5];
                domains = new int[array.Length - 5];
                for (int i = 6; i < array.Length; i++)
                {
                    string[] pair = array[i].Split(commaSep);
                    urls[i - 6] = Int32.Parse(pair[0]);
                    domains[i - 6] = Int32.Parse(pair[1]);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override bool writeToFile(BinaryWriter writer)
        {
            try
            {
                writer.Write(type);
                writer.Write(sessionId);
                writer.Write(time);
                writer.Write(serpId);
                writer.Write(queryId);

                writer.Write(terms.Length);
                for (int i = 0; i < terms.Length; i++)
                    writer.Write(terms[i]);

                writer.Write(urls.Length);
                for (int i = 0; i < urls.Length; i++)
                {
                    writer.Write(urls[i]);
                    writer.Write(domains[i]);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}