using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which use a trigger (such as traps, barriers, ...).
/// </summary>
public interface ITriggerable
{

    /// <summary>
    /// Handles the object when triggered by a player/enemy.
    /// </summary>
    void Trigger(Collider other);
}
