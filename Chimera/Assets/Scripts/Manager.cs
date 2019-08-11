using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic manager class that other managers inherit from.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T GetManager()
    {
        return FindObjectOfType<T>();
    }
}
