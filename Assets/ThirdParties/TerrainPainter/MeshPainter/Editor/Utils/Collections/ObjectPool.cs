using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMP.Collections
{
    public class ObjectPool<T> : IDisposable where T : class
    {
        Stack<T> availableObj;
        List<T> pool;

        Func<T> createFunc;
        Action<T> onGet;
        Action<T> onRelease;
        Action<T> onDestroy;
        public int capacity;
        int count = 0;

        public ObjectPool(
            Func<T> createFunc, 
            Action<T> onGet = null, 
            Action<T> onRelease = null, 
            Action<T> onDestroy = null, 
            int capacity = 0)
        {
            if(capacity > 0)
            {
                availableObj = new Stack<T>(capacity);
                pool = new List<T>(capacity);
            }
            else
            {
                availableObj = new Stack<T>();
                pool = new List<T>();
            }
            this.createFunc = createFunc;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.onDestroy = onDestroy;
            this.capacity = capacity;
        }

        public bool TryGet(out T obj)
        {
            if(availableObj.Count == 0)
            {
                if(capacity > 0 && count >= capacity)
                {
                    obj = null;
                    return false;
                }
                else
                {
                    obj = createFunc();
                    pool.Add(obj);
                }
            }
            else
            {
                obj = availableObj.Pop();
            }
            onGet?.Invoke(obj);
            return true;
        }

        public void Release(T obj)
        {
            onRelease?.Invoke(obj);
            availableObj.Push(obj);
        }

        public void Dispose()
        {
            foreach(var obj in pool)
            {
                onDestroy?.Invoke(availableObj.Pop());
            }
        }
    }
}