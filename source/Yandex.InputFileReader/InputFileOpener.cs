using System;
using Yandex.Utils;
using Yandex.Utils.UserActions;

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
                Metadata metadata = new Metadata();
                QueryAction queryAction = new QueryAction();
                Click click = new Click();

                float length = binaryReader.reader.BaseStream.Length/100.0f;

                int lineCounter = 0;

                _reader.onBeginRead();

                int type = binaryReader.PeekChar();
                while (type > -1)
                {
                    if (++lineCounter%100000 == 0)
                        Console.Write("                 \rRead: {0} %\r",
                            (binaryReader.reader.BaseStream.Position/length).ToString("0.000"));

                    switch (type)
                    {
                        case 0:
                        {
                            metadata.type = binaryReader.ReadByte();
                            metadata.sessionId = binaryReader.ReadInt32();
                            metadata.day = binaryReader.ReadInt32();
                            metadata.userId = binaryReader.ReadInt32();

                            _reader.onMetadata(metadata);
                            break;
                        }
                        case 1:
                        case 2:
                        {
                            queryAction.type = binaryReader.ReadByte();
                            queryAction.sessionId = binaryReader.ReadInt32();
                            queryAction.time = binaryReader.ReadInt32();
                            queryAction.serpid = binaryReader.ReadInt32();
                            queryAction.queryId = binaryReader.ReadInt32();

                            int nTerms = binaryReader.ReadInt32();
                            queryAction.nTerms = nTerms;
                            if (queryAction.terms == null || queryAction.terms.Length < nTerms)
                                queryAction.terms = new int[nTerms];

                            for (int i = 0; i < nTerms; i++)
                                queryAction.terms[i] = binaryReader.ReadInt32();

                            int nUrls = binaryReader.ReadInt32();
                            queryAction.nUrls = nUrls;
                            if (queryAction.urls == null || queryAction.urls.Length < nUrls)
                            {
                                queryAction.urls = new int[nUrls];
                                queryAction.domains = new int[nUrls];
                            }

                            for (int i = 0; i < nUrls; i++)
                            {
                                queryAction.urls[i] = binaryReader.ReadInt32();
                                queryAction.domains[i] = binaryReader.ReadInt32();
                            }

                            _reader.onQueryAction(queryAction);
                            break;
                        }
                        case 3:
                        {
                            click.type = binaryReader.ReadByte();
                            click.sessionId = binaryReader.ReadInt32();
                            click.time = binaryReader.ReadInt32();
                            click.serpid = binaryReader.ReadInt32();
                            click.urlId = binaryReader.ReadInt32();

                            _reader.onClick(click);
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