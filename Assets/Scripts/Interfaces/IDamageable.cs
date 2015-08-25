using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which can take damage (for example the players and the enemies).
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Draws the object some damage and lowers the health.
    /// </summary>
    /// <param name="damage">The damage dealt.</param>
    /// <param name="damageDealer">The damage dealer</param>
    void TakeDamage(int damage, MonoBehaviour damageDealer);

    /// <summary>
    /// Lowers the health to zero.
    /// </summary>
    void InstantKill();

    /// <summary>
    /// Gets or sets the actual health value.
    /// </summary>
    int Health
    {
        get;
        set;
    }
}