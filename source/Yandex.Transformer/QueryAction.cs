using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Transformer
{
    class QueryAction : UserAction
    {
        static char[] commaSep = new char[] { ',' };

        byte type;
        int sessionId;
        int time;
        int serpId;
        int queryId;
        int[] terms;
        int[] urls;
        int[] domains;

        public override bool readData(string[] array)
        {
            try
            {
                type = (byte)(array[2][0] == 'Q' ? 1 : 2);
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

        public override bool writeToFile(BufferedBinaryWriter writer)
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
