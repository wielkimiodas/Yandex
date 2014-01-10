using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.InputFileReader
{
    class TermStats : InputFileReader
    {
        HashSet<int> terms = new HashSet<int>();
        int maxTermId = Int32.MinValue;

        public override void onQueryAction(Utils.UserActions.QueryAction queryAction)
        {
            for (int i = 0; i < queryAction.nTerms; i++)
            {
                terms.Add(queryAction.terms[i]);
                if (maxTermId < queryAction.terms[i])
                    maxTermId = queryAction.terms[i];
            }
        }

        public override void onEndRead()
        {
            Console.WriteLine("Count:\t" + terms.Count);
            Console.WriteLine("Max:\t" + maxTermId);
        }
    }
}
