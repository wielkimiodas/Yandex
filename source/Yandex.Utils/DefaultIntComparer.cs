using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Utils
{
    public class DefaultIntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return x - y;
        }
    }
}
