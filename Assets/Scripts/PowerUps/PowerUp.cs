using UnityEngine;
using System.Collections;

public abstract class PowerUp :  MonoBehaviour,IUsable {

	// defines how long the power up is active
	public float powerUpActiveTime;

	// defines if the spawned powerup is 
	public bool isPending;

	// if false, then the code use() is not executeable
	protected bool isActive = true;

	public virtual void Use(){}

	public string Name
	{
		get
		{
			return gameObject.name;
		}
		set
		{ 
			gameObject.name = value;
		}
	}
}
