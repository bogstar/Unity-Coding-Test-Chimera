using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic static utilities class.
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Randomize list order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
