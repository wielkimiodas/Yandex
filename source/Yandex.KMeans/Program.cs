using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace Yandex.KMeans
{
    class Program
    {
        static void Main(string[] args)
        {
            KMeans.DoKMeans(PathResolver.UserMatrix, @"D:\Downloads\EDWD\usersFinal.txt");
        }
    }
}
