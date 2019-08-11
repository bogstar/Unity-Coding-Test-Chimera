using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Allegiance
{
    Player,
    Enemy
}

public enum Phase
{
    None,
    Select,
    Move,
    Moving,
    Action,
    EndMatch
}