using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Transformer
{
    class Metadata : UserAction
    {
        byte type;
        int sessionId;
        int day;
        int userId;

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
