using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which are collectible(powerUps,...).
/// </summary>
public interface ICollectible {

	/// <summary>
	/// Handles the destruction of the collectible object after the lifetime.
	/// </summary>
	void DestroyCollectible(float time);

}
