using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class InputFileReader : IDisposable
    {
        public virtual void Dispose() { }

        public virtual void onBeginRead() { }

        public virtual void onMetadata(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // DAY
            reader.ReadInt32();

            // USER
            reader.ReadInt32();
        }

        public virtual void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            reader.ReadInt32();
            // QUERYID
            reader.ReadInt32();

            for (int i = reader.ReadInt32(); i > 0; i--)
                // TERM ID
                reader.ReadInt32();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();
            }
        }

        public virtual void onClick(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();

            // SERPID
            reader.ReadInt32();

            // URL
            reader.ReadInt32();
        }

        public virtual void onEndRead() { }
    }
}
