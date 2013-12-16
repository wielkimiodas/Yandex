using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class Click : UserAction
    {
        public byte type { get; protected set; }
        public int sessionId { get; protected set; }
        public int time { get; protected set; }
        public int serpId { get; protected set; }
        public int urlId { get; protected set; }

        public Click()
        {
        }

        public Click(int sessionId, int time, int serpId, int urlId)
        {
            this.type = 3;
            this.sessionId = sessionId;
            this.time = time;
            this.serpId = serpId;
            this.urlId = urlId;
        }

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

        public override bool writeToFile(BinaryWriter writer)
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