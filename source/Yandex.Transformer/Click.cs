using System;

namespace Yandex.Transformer
{
    internal class Click : UserAction
    {
        private byte type;
        private int sessionId;
        private int time;
        private int serpId;
        private int urlId;

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