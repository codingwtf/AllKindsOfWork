using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMP.Collections
{
    public interface IRandomAccessEnumerable<TKey, TValue> : IEnumerable<TValue>
    {
        TValue this[TKey idx] { get; }
        int Count { get; }
    }

    public interface IRWRandomAccessEnumerable<TKey, TValue> : IRandomAccessEnumerable<TKey, TValue>
    {
        new TKey this[TKey idx] { set; }
    }

    public class RandomAccessCollection<TValue> : IRandomAccessEnumerable<int, TValue>
    {
        public delegate TValue Indexer(int idx);
        public delegate int GetCount();
        IEnumerable<TValue> collection;
        Indexer indexer;
        GetCount getCount;

        public RandomAccessCollection(IEnumerable<TValue> collection, Indexer indexer, GetCount getCount)
        {
            this.collection = collection;
            this.indexer = indexer;
            this.getCount = getCount;
        }

        public RandomAccessCollection(TValue[] array)
        {
            this.collection = array;
            this.indexer = (idx) => array[idx];
            this.getCount = () => array.Length;
        }

        public TValue this[int idx] => indexer(idx);

        public int Count => getCount();

        public IEnumerator<TValue> GetEnumerator() => collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => collection.GetEnumerator();
    }
}