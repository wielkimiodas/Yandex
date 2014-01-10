using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Coprimes
{
    public class CoPrime
    {
        public static Tuple<long, long> GetCoprimeTuple()
        {
            var random = new Random();
            const long terms = 10000; //minhash.getparams
            long first = random.Next(100, 1000000);

            if (first%2 == terms%2) first++;

            while (!IsPairCoPrime(first, terms))
            {
                first+=2;
            }
            long second = first + 1;
            while (!IsPairCoPrime(first, second))
            {
                second+=2;
            }
            return new Tuple<long, long>(first, second);
        }

        public static bool IsPairCoPrime(long a, long b)
        {
            return GCD(a, b) == 1;
        }

        public static long GCD(long a, long b)
        {
            while (b != 0)
            {
                long t = b;
                b = a % b;
                a = t;
            }
            return a;
        }
    }
}