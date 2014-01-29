using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    public class Counter : InputFileReader
    {
        private int maxUrlIndex = 0;
        
        public override void onQueryAction(QueryAction queryAction)
        {
            for (int i = 0; i < queryAction.nUrls; i++)
            {
                if (queryAction.urls[i] > maxUrlIndex)
                    maxUrlIndex = queryAction.urls[i];
            }
        }

        public override void onEndRead()
        {
            Console.WriteLine(maxUrlIndex);
        }
    }
}
