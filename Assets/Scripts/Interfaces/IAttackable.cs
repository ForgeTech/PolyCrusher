using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which can attack in some way.
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// Handles the shoot mechanism.
    /// </summary>
    void Shoot();

    /// <summary>
    /// Handles the close quarter attack (if neccesary).
    /// </summary>
    void Attack();

    /// <summary>
    /// Specifies if the object can shoot or not.
    /// </summary>
    bool CanShoot
    {
        get;
        set;
    }
}