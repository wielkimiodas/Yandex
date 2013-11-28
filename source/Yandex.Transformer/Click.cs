using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Transformer
{
    class Click : UserAction
    {
        byte type;
        int sessionId;
        int time;
        int serpId;
        int urlId;

        public override bool readData(string[] array)
        {
            try
            {
                type = 3;
                sessionId = Int32.Parse(array[0]);
                time = Int32.Parse(array[1]);
                serpId = Int32.Parse(array[3]);
                urlId = Int32.Parse(array[4]);
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
                writer.Write(urlId);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
