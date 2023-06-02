using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Type = System.Type;

namespace UMP.Utils
{
    public static class SelectionUtility
    {
        public static void Add(int instanceID)
        {
            typeof(Selection)
                .GetMethod(
                    "Add",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(int) },
                    null)
                .Invoke(null, new object[] { instanceID });
        }
        public static void Add(Object obj)
        {
            if (obj != null)
            {
                Add(obj.GetInstanceID());
            }
        }
        public static void Remove(int instanceID)
        {
            typeof(Selection)
                .GetMethod(
                    "Remove",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(int) },
                    null)
                .Invoke(null, new object[] { instanceID });
        }
        public static void Remove(Object obj)
        {
            if (obj != null)
            {
                Remove(obj.GetInstanceID());
            }
        }
    }
}