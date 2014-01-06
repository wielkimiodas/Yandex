using System;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class InputFileOpener : IDisposable
    {
        private readonly string _filename;

        private readonly InputFileReader _reader;

        public InputFileOpener(string filename, InputFileReader reader)
        {
            _filename = filename;
            _reader = reader;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public void Read()
        {
            using (var binaryReader = new BufferedBinaryReader(_filename))
            {
                float length = binaryReader.reader.BaseStream.Length/100.0f;

                int lineCounter = 0;

                _reader.onBeginRead();

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
                            _reader.onMetadata(binaryReader);
                            break;
                        }
                        case 1:
                        case 2:
                        {
                            _reader.onQueryAction(binaryReader);
                            break;
                        }
                        case 3:
                        {
                            _reader.onClick(binaryReader);
                            break;
                        }
                    }

                    type = binaryReader.PeekChar();
                }

                Console.Write("                  \r");

                _reader.onEndRead();
            }
        }
    }
}