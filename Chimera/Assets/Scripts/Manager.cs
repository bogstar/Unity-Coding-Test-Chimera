using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T GetManager()
    {
        return FindObjectOfType<T>();
    }
}
