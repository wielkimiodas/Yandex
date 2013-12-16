using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public class Metadata : UserAction
    {
        public byte type { get; protected set; }
        public int sessionId { get; protected set; }
        public int day { get; protected set; }
        public int userId { get; protected set; }

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

        public override bool readData(string[] array)
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

        public override bool writeToFile(BinaryWriter writer)
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