using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Utils
{
    public class DynamicSortedList<T> : ICollection<T> where T : IComparable
    {
        List<T> list;
        Comparison<T> comparison;

        public DynamicSortedList(Comparison<T> comparison)
        {
            this.comparison = comparison;
            list = new List<T>();
        }

        public DynamicSortedList(ICollection<T> collection, Comparison<T> comparison)
        {
            list = new List<T>(collection);
            this.comparison = comparison;

            list.Sort(comparison);
        }

        public void Add(T item)
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }

            int min = -1;
            int max = list.Count;

            int index = 0;

            while (max - min >= 2)
            {
                index = (max - min) / 2 + min;

                int comp = comparison.Invoke(list[index], item);

                if (comp == 0)
                    break;

                if (comp < 0)
                    min = index;
                else
                    max = index;
            }

            int cmp = comparison.Invoke(list[index], item);
            if (cmp > 0)
                list.Insert(index, item);
            else
                list.Insert(index + 1, item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
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
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
