using UnityEngine;
using System.Collections;

public abstract class BaseItem : MonoBehaviour, ICollectible {

	#region Class Members
	// Name of the item
	[SerializeField]
	protected string ItemName;

	// Array of powerups 
	//NOT IMPLEMENTED YET
	//[SerializeField]
	protected PowerUp[] powerUps;
	#endregion
    
	#region Class Methods
	// Use this for initialization
	void Start () {
		gameObject.name = this.name;
	}

	// Handles the destroy mechanism (decrement the lifetime of the object on script start)
	public virtual void DestroyCollectible(float destroyLifeTime){
		// destroy gameobject to prevent data overload after a certain time
		Destroy(gameObject,destroyLifeTime);
	}

	#endregion
}
