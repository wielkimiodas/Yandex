using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class InputFileOpener : IDisposable
    {
        string filename;

        InputFileReader reader;

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
                int lineCounter = 0;

                reader.onBeginRead();

                int type = binaryReader.PeekChar();
                while (type > -1)
                {
                    lineCounter++;

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

                reader.onEndRead();
            }
        }
    }
}
