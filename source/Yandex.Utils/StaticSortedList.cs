using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Utils
{
    /// <summary>
    /// Przechowuje posortowaną listę.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StaticSortedList<T> : ICollection<T> where T : IComparable
    {
        List<T> list;
        bool isCorrect = true;
        Comparison<T> comparison;

        public StaticSortedList(Comparison<T> comparison)
        {
            this.comparison = comparison;
            list = new List<T>();
        }

        public StaticSortedList(ICollection<T> collection, Comparison<T> comparison)
        {
            list = new List<T>(collection);
            this.comparison = comparison;

            list.Sort(comparison);
        }

        private void Check()
        {
            if (!isCorrect)
            {
                list.Sort(comparison);
                isCorrect = true;
            }
        }

        public void Add(T item)
        {
            list.Add(item);
            isCorrect = false;
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            Check();

            if (list.Count == 0)
                return false;

            int min = -1;
            int max = list.Count;

            int index = 0;

            while (max - min >= 2)
            {
                index = (max - min) / 2 + min;

                int comp = comparison.Invoke(list[index], item);

                if (comp == 0)
                    return true;

                if (comp < 0)
                    min = index;
                else
                    max = index;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Check();
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            Check();
            return list.GetEnumerator();
        }
    }
}
