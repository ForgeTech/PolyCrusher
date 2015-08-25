using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which are moveable (With player input or AI, ...).
/// </summary>
public interface IMoveable
{
    /// <summary>
    /// Handles when the object should move (for example when there is user input).
    /// </summary>
    void HandleMovement();


    /// <summary>
    /// Handles the actual movement with a speed in a certain direction.
    /// </summary>
    /// <param name="speedFactor">Speed of the movement.</param>
    /// <param name="direction">Direction.</param>
    void ManipulateMovement(float speedFactor, Vector3 direction);
}
