using UnityEngine;
using System.Collections;

public delegate void PowerUpCollectedHandler();

public class PowerUpItem :   BaseItem {

	#region Class Members
	// enum of the Power Ups to show it as a dropdown menu
	public enum PowerUpEnum{
		addHealth,addEnergy,weaponDamage,weaponFireRate,addMaxHealth,addMaxEnergy
	}

	// if true, the value of the power up will be added permanently to the player
	[SerializeField]
	protected bool addPermanently = true;

	// Time after the power up is inactive
	[SerializeField]
	private float outlastTime = 5;

	// Lifetime of the pending object
	[SerializeField]
	private float pendingTime = 2f;

	// Changeable value of the powerup
	[SerializeField]
	private int powerUpValue = 5;

	[SerializeField]
	private PowerUpEnum type;

    [Space(5)]
    [Header("Particles")]
    [SerializeField]
    protected GameObject pickUpParticles;

	// Event handler for the collectible collected.
	public static event PowerUpCollectedHandler PowerUpCollected;
    #endregion

    #region Class Methods
    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
    }

	// Handles the pick up mechanism
	void OnTriggerEnter(Collider collider) {
		if (collider.tag == "Player"){
			// get the BasePlayer of the Game Object
			BasePlayer player = collider.GetComponent<BasePlayer>();

			if (type == PowerUpEnum.addHealth){
				PowerUpAddHealth AddHealth = player.gameObject.AddComponent<PowerUpAddHealth>();
				AddHealth.Use (player);
				//Destroy (AddHealth);
			}
			if (type == PowerUpEnum.addEnergy){
				PowerUpAddEnergy AddEnergy = player.gameObject.AddComponent<PowerUpAddEnergy>();
				AddEnergy.Use (player);
				//Destroy (AddEnergy);
			}

			if (type == PowerUpEnum.weaponDamage){
				if (addPermanently || player.gameObject.GetComponent<PowerUpWeaponDamage>() == null){
					PowerUpWeaponDamage AddDamage = player.gameObject.AddComponent<PowerUpWeaponDamage>();
					AddDamage.Use (player,outlastTime, powerUpValue, addPermanently);
					//Destroy (AddDamage);

				} else if (!addPermanently){
					PowerUpWeaponDamage AddDamage = player.gameObject.GetComponent<PowerUpWeaponDamage>();
					AddDamage.breakAndRestart();
				}
			}
			if (type == PowerUpEnum.weaponFireRate){
				if (addPermanently || player.gameObject.GetComponent<PowerUpWeaponFireRate>() == null){
					PowerUpWeaponFireRate AddFireRate = player.gameObject.AddComponent<PowerUpWeaponFireRate>();
					AddFireRate.Use (player,outlastTime, powerUpValue, addPermanently);
					//Destroy (AddFireRate);
				} else if (!addPermanently){
					PowerUpWeaponFireRate AddFireRate = player.gameObject.GetComponent<PowerUpWeaponFireRate>();
					AddFireRate.breakAndRestart();
				}
			}

			if (type == PowerUpEnum.addMaxHealth){
				PowerUpAddMaxHealth AddHealth = player.gameObject.AddComponent<PowerUpAddMaxHealth>();
				AddHealth.Use (player, powerUpValue);
				//Destroy (AddHealth);
			}
			if (type == PowerUpEnum.addMaxEnergy){
				PowerUpAddMaxEnergy AddEnergy = player.gameObject.AddComponent<PowerUpAddMaxEnergy>();
				AddEnergy.Use (player, powerUpValue);
				//Destroy (AddEnergy);
			}

			// Trigger Event.
			CollectingPowerUp();

			// set the player as parent gameobject
			GameObject playerParent = collider.gameObject;
			int i = 0;
			while (i < playerParent.transform.childCount){
				GameObject childObject = playerParent.transform.GetChild(i).gameObject;
				if (childObject != null){
					if (childObject.GetComponent<PowerUpItem>() != null){
						PowerUpItem powerUpComponent = childObject.GetComponent<PowerUpItem>();
						powerUpComponent.StopAllCoroutines();
						//childObject.SetActive(false);
						Destroy(childObject);
					}
				}
				i++;
			}
			// Let the object pend over the player
			GameObject pendingObject = Instantiate(gameObject,new Vector3(playerParent.transform.position.x , playerParent.transform.position.y + 2.25f, playerParent.transform.position.z), transform.rotation) as GameObject;
			//pendingObject.GetComponent<RotateObject>();
			SphereCollider pendingSphereCollider = pendingObject.GetComponent<SphereCollider>();
			pendingSphereCollider.enabled = false;
			//Vector3 pendingObjectOriginScale = pendingObject.transform.localScale;
			pendingObject.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
			//StartCoroutine(pendingObject.transform.ScaleTo(new Vector3(1f,1f,1f), 0.5f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
			StartCoroutine(Tween(pendingObject.transform, new Vector3(1f,1f,1f), 0.5f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
			StartCoroutine(TweenBack(pendingObject.transform, pendingTime));
			pendingObject.transform.SetParent(playerParent.transform, true);
			//Debug.Log (pendingObject.transform.localScale);
			Destroy(pendingObject, pendingTime);

            // Particle
            if (pickUpParticles != null)
                Instantiate(pickUpParticles, transform.position, pickUpParticles.transform.rotation);

            // send event
            new Event(Event.TYPE.powerup).addWave().addCharacter(player.PlayerName).addPos(this.transform).addLevel().send();		
			MeshRenderer meshRenderer =transform.GetComponent<MeshRenderer>();

			// if the mesh renderer is in a childobject of the gameobject, then search all renderers of the child gameobject 
			// and store it into an Renderer array, then disable all
			if (meshRenderer == null){
				Renderer[] meshRendererArray = GetComponentsInChildren<Renderer>();
				foreach(Renderer renderer in meshRendererArray){
					renderer.enabled = false;
				}
			} else {
				// deactivate mesh renderer to hide the collectible item until it is destroyed
				meshRenderer.enabled = false;
			}

			ParticleSystem particleSystem =transform.GetComponent<ParticleSystem>();
			if (particleSystem == null){
				ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
				foreach(ParticleSystem particles in particleSystems){
					particles.emissionRate = 0.0F;
				}
			} else {
				// set particle emisiionrate to 0.0F to hide the collectible item until it is destroyed
				particleSystem.emissionRate = 0.0F;
			}
			
			// deactivate collider to prevent from multicasting the power up
			SphereCollider sphereCollider =transform.GetComponent<SphereCollider>();
			sphereCollider.enabled = false;

			DestroyCollectible(outlastTime);

		}
	}

	protected void CollectingPowerUp()
	{
		if (PowerUpCollected != null)
			PowerUpCollected();
	}

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected void ResetValues()
    {
        PowerUpCollected = null;
    }

	// self implemented tweening method is needed to make it able to stop via StopAllCoroutines();
	public IEnumerator Tween(Transform transform, Vector3 target, float duration, Easer ease)
	{
		float elapsed = 0;
		var start = transform.localScale;
		var range = target - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			if (transform != null){
				transform.localScale = start + range * ease(elapsed / duration);
				yield return 0;
			}
		}
		if (transform != null){
			transform.localScale = target;
		}
	}

	public IEnumerator TweenBack(Transform pendingObject, float time)
	{
		float TWEENBACK_TIME = 0.3f;
		if (transform != null){
			// Wait pending time - tweentime to let it fully tween back
			yield return new WaitForSeconds(time - TWEENBACK_TIME);
			if (pendingObject != null){
				StartCoroutine(Tween(pendingObject.transform, new Vector3(0.05f,0.05f,0.05f), TWEENBACK_TIME, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
			}
		}
	}

	#endregion
}