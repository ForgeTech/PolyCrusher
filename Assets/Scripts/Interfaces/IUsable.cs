using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects which are usable(weapons, abilities, powerups, ...).
/// </summary>
public interface IUsable {
	
	/// <summary>
	/// Handles the object when a player uses an object .
	/// </summary>
	void Use();
	
	/// <summary>
	/// Specifies if the name of the object.
	/// </summary>
	string Name
	{
		get;
		set;
	}
	
}
