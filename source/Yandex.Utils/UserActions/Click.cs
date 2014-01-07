using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class Click : UserAction
    {
        public byte type;
        public int sessionId;
        public int time;
        public int serpid;
        public int urlId;

        public Click()
        {
        }

        public Click(int sessionId, int time, int serpId, int urlId)
        {
            this.type = 3;
            this.sessionId = sessionId;
            this.time = time;
            this.serpid = serpId;
            this.urlId = urlId;
        }

        public override bool ReadData(string[] array)
        {
            try
            {
                type = 3;
                sessionId = Int32.Parse(array[0]);
                time = Int32.Parse(array[1]);
                serpid = Int32.Parse(array[3]);
                urlId = Int32.Parse(array[4]);
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
                writer.Write(type);
                writer.Write(sessionId);
                writer.Write(time);
                writer.Write(serpid);
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