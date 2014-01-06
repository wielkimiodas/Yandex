using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class QueryAction : UserAction
    {
        private static readonly char[] CommaSep = {','};

        public byte Type { get; protected set; }
        public int SessionId { get; protected set; }
        public int Time { get; protected set; }
        public int SerpId { get; protected set; }
        public int QueryId { get; protected set; }
        public int[] Terms { get; protected set; }
        public int[] Urls { get; protected set; }
        public int[] Domains { get; protected set; }

        public QueryAction()
        {
        }

        public QueryAction(byte type, int sessionId, int time, int serpId, int queryId, int[] terms, int[] urls,
            int[] domains)
        {
            Type = type;
            SessionId = sessionId;
            Time = time;
            SerpId = serpId;
            QueryId = queryId;
            Terms = terms;
            Urls = urls;
            Domains = domains;
        }

        public override bool ReadData(string[] array)
        {
            try
            {
                Type = (byte) (array[2][0] == 'Q' ? 1 : 2);
                SessionId = Int32.Parse(array[0]);
                Time = Int32.Parse(array[1]);
                SerpId = Int32.Parse(array[3]);
                QueryId = Int32.Parse(array[4]);

                string[] termsArray = array[5].Split(CommaSep);
                Terms = new int[termsArray.Length];
                for (int i = 0; i < termsArray.Length; i++)
                    Terms[i] = Int32.Parse(termsArray[i]);

                Urls = new int[array.Length - 5];
                Domains = new int[array.Length - 5];
                for (int i = 6; i < array.Length; i++)
                {
                    string[] pair = array[i].Split(CommaSep);
                    Urls[i - 6] = Int32.Parse(pair[0]);
                    Domains[i - 6] = Int32.Parse(pair[1]);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override bool WriteToFile(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write(SessionId);
                writer.Write(Time);
                writer.Write(SerpId);
                writer.Write(QueryId);

                writer.Write(Terms.Length);
                for (int i = 0; i < Terms.Length; i++)
                    writer.Write(Terms[i]);

                writer.Write(Urls.Length);
                for (int i = 0; i < Urls.Length; i++)
                {
                    writer.Write(Urls[i]);
                    writer.Write(Domains[i]);
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