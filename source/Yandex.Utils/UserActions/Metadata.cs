using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class Metadata : UserAction
    {
        public byte type;
        public int sessionId;
        public int day;
        public int userId;

        public Metadata()
        {
        }

        public Metadata(int sessionId, int day, int userId)
        {
            this.type = 0;
            this.sessionId = sessionId;
            this.day = day;
            this.userId = userId;
        }

        public override bool ReadData(string[] array)
        {
            try
            {
                type = 0;
                sessionId = Int32.Parse(array[0]);
                day = Int32.Parse(array[2]);
                userId = Int32.Parse(array[3]);
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
                writer.Write(day);
                writer.Write(userId);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}