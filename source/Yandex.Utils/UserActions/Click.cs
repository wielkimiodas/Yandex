using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class Click : UserAction
    {
        public byte Type { get; protected set; }
        public int SessionId { get; protected set; }
        public int Time { get; protected set; }
        public int SerpId { get; protected set; }
        public int UrlId { get; protected set; }

        public Click()
        {
        }

        public Click(int sessionId, int time, int serpId, int urlId)
        {
            Type = 3;
            SessionId = sessionId;
            Time = time;
            SerpId = serpId;
            UrlId = urlId;
        }

        public override bool ReadData(string[] array)
        {
            try
            {
                Type = 3;
                SessionId = Int32.Parse(array[0]);
                Time = Int32.Parse(array[1]);
                SerpId = Int32.Parse(array[3]);
                UrlId = Int32.Parse(array[4]);
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
                writer.Write(UrlId);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}