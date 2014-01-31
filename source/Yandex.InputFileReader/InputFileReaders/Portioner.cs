using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.LogPortioner
{
    class Portioner : InputFileReader.InputFileReader
    {
        private const int DaysCount = 27;
        private long _sessions;
        private BinaryWriter[] _fileParts;
        private int currentDay = -1;


        public override void onBeginRead()
        {
            _fileParts = new BinaryWriter[DaysCount];

            _sessions = 0;

            if (!Directory.Exists(PathResolver.DataPartsFolder))
                Directory.CreateDirectory(PathResolver.DataPartsFolder);

            for (int i = 0; i < _fileParts.Length; i++)
            {
                _fileParts[i] = new BinaryWriter(File.Open(PathResolver.DataPartsFolder + "part" + (i + 1), FileMode.CreateNew));
            }
        }

        public override void onMetadata(Metadata metadata)
        {
            currentDay = metadata.day - 1;
            metadata.WriteToStream(_fileParts[currentDay]);
        }

        public override void onClick(Click click)
        {
            click.WriteToStream(_fileParts[currentDay]);
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            queryAction.WriteToStream(_fileParts[currentDay]);
        }

        public override void onEndRead()
        {
            foreach (var streamWriter in _fileParts)
            {
                streamWriter.Close();
            }
        }
    }
}
