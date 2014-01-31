using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class QueryAction : UserAction
    {
        private static readonly char[] CommaSep = {','};

        public byte type;
        public int sessionId;
        public int time;
        public int serpid;
        public int queryId;
        public int nTerms;
        public int[] terms;
        public int nUrls;
        public int[] urls;
        public int[] domains;

        public QueryAction()
        {
        }

        public QueryAction(byte type, int sessionId, int time, int serpId, int queryId, int[] terms, int[] urls,
            int[] domains)
        {
            this.type = type;
            this.sessionId = sessionId;
            this.time = time;
            this.serpid = serpId;
            this.queryId = queryId;
            this.nTerms = terms.Length;
            this.terms = terms;
            this.nUrls = urls.Length;
            this.urls = urls;
            this.domains = domains;
        }

        public override bool ReadData(string[] array)
        {
            try
            {
                type = (byte) (array[2][0] == 'Q' ? 1 : 2);
                sessionId = Int32.Parse(array[0]);
                time = Int32.Parse(array[1]);
                serpid = Int32.Parse(array[3]);
                queryId = Int32.Parse(array[4]);

                string[] termsArray = array[5].Split(CommaSep);
                terms = new int[termsArray.Length];
                for (int i = 0; i < termsArray.Length; i++)
                    terms[i] = Int32.Parse(termsArray[i]);

                urls = new int[array.Length - 5];
                domains = new int[array.Length - 5];
                for (int i = 6; i < array.Length; i++)
                {
                    string[] pair = array[i].Split(CommaSep);
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

        public override bool WriteToStream(BinaryWriter writer)
        {
            try
            {
                writer.Write(type);
                writer.Write(sessionId);
                writer.Write(time);
                writer.Write(serpid);
                writer.Write(queryId);

                writer.Write(nTerms);
                for (int i = 0; i < nTerms; i++)
                    writer.Write(terms[i]);

                writer.Write(nUrls);
                for (int i = 0; i < nUrls; i++)
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