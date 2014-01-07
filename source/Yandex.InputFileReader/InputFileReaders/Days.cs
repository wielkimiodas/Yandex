using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class Days : InputFileReader
    {
        Dictionary<int, int> counts;

        public override void onBeginRead()
        {
            counts = new Dictionary<int, int>();
        }

        public override void onMetadata(Metadata metadata)
        {
            if (counts.ContainsKey(metadata.day))
                counts[metadata.day]++;
            else
                counts.Add(metadata.day, 1);
        }

        public override void onEndRead()
        {
            foreach (var element in counts)
                Console.WriteLine(element.Key + ":\t" + element.Value);
        }
    }
}
