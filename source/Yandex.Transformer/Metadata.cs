using System;

namespace Yandex.Transformer
{
    internal class Metadata : UserAction
    {
        private byte type;
        private int sessionId;
        private int day;
        private int userId;

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

        public override bool writeToFile(BufferedBinaryWriter writer)
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