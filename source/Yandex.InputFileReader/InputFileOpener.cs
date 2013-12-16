using System;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class InputFileOpener : IDisposable
    {
        private string filename;

        private InputFileReader reader;

        public InputFileOpener(string filename, InputFileReader reader)
        {
            this.filename = filename;
            this.reader = reader;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public void read()
        {
            using (BufferedBinaryReader binaryReader = new BufferedBinaryReader(filename))
            {
                float length = binaryReader.reader.BaseStream.Length/100.0f;

                int lineCounter = 0;

                reader.onBeginRead();

                int type = binaryReader.PeekChar();
                while (type > -1)
                {
                    lineCounter++;
                    if (lineCounter%100000 == 0)
                        Console.Write("                 \rRead: {0} %\r",
                            (binaryReader.reader.BaseStream.Position/length).ToString("0.000"));

                    switch (type)
                    {
                        case 0:
                        {
                            reader.onMetadata(binaryReader);
                            break;
                        }
                        case 1:
                        case 2:
                        {
                            reader.onQueryAction(binaryReader);
                            break;
                        }
                        case 3:
                        {
                            reader.onClick(binaryReader);
                            break;
                        }
                    }

                    type = binaryReader.PeekChar();
                }

                Console.Write("                  \r");

                reader.onEndRead();
            }
        }
    }
}