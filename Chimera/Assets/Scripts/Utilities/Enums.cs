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
    /// <summary>
    /// Ended turn, waiting for Game Manager to process next turn.
    /// </summary>
    NextTurn,

    /// <summary>
    /// Player can freely select units and preview them on board.
    /// </summary>
    Select,

    /// <summary>
    /// Player takes action for a selected unit.
    /// </summary>
    Action,

    /// <summary>
    /// Unit is performing an action.
    /// </summary>
    Animation,

    /// <summary>
    /// Game has ended.
    /// </summary>
    EndMatch
}