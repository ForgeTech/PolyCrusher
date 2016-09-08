using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler for collecting of a powerup.
/// </summary>
public delegate void PowerUpCollectedHandler();

/// <summary>
/// PowerUp Item class for the powerups and their selection technique.
/// </summary>
public class PowerUpItem : BaseItem
{

    #region Class Members

    // Enum of the Power Ups to show it as a dropdown menu
    public enum PowerUpEnum
    {
        addHealth, addEnergy, weaponDamage, weaponFireRate, addMaxHealth, addMaxEnergy, lineCut, mangoExplosion
    }

    // If true, the value of the power up will be added permanently to the player
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

    // PowerUpEnum variable
    [SerializeField]
    private PowerUpEnum type;

    [SerializeField]
    [Tooltip("Determines if the power up is part of the Pissing Pete or if it is a single power up.")]
    private bool singlePowerUp = false;

    // Add some spacing in the Unity Inspector
    [Space(5)]
    // Add a header above some fields in the Unity Inspector
    [Header("Particles")]
    [SerializeField]
    // Particles which are staying in the air after picking up
    protected GameObject pickUpParticles;
    
    [Space(5)]
    [Header("Mango Powerup")]
    [SerializeField]
    protected GameObject explosionParticles;

    [SerializeField]
    protected float range = 5f;

    [SerializeField]
    protected AudioClip clip;

    // Event handler for the collectible collected.
    public static event PowerUpCollectedHandler PowerUpCollected;
    #endregion

    #region Class Methods
    // Reset values before calling Start()
    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
    }

    /// <summary>
    /// Handles the collecting mechanism.
    /// </summary>
    /// <param name="collider">Colliding object parameter.</param>
    /// <returns></returns>
	void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            // Get the BasePlayer of the Game Object
            BasePlayer player = collider.GetComponent<BasePlayer>();
            player.PowerUpPickUpRumble();

            //==========================SETTING_PLAYERVALUES_TO_MAX_POWERUPS_START=======================
            // Check if the colliding object is a PowerUpEnam.addHealth
            if (type == PowerUpEnum.addHealth)
            {

                // Create new PowerUpAddHealth variable and add component of the PowerUpAddHealth
                PowerUpAddHealth AddHealth = player.gameObject.AddComponent<PowerUpAddHealth>();

                // Call Use on the player which has collected the powerup
                AddHealth.Use(player);
                //Destroy (AddHealth);
            }

            if (type == PowerUpEnum.addEnergy)
            {
                PowerUpAddEnergy AddEnergy = player.gameObject.AddComponent<PowerUpAddEnergy>();
                AddEnergy.Use(player);
                //Destroy (AddEnergy);
            }
            //==========================SETTING_PLAYERVALUES_TO_MAX_POWERUPS_END=========================

            //==================TEMPORARY_OR_PERMANENTLY_ADD_POWERUPS_START==============================
            if (type == PowerUpEnum.weaponDamage)
            {
                // If the gamedesigner want to add permanently the weapon damage OR there is no PowerUpWeaponDamage component on the player...
                if (addPermanently || player.gameObject.GetComponent<PowerUpWeaponDamage>() == null)
                {
                    // Then create a PowerUpWeaponDamage variable and...
                    PowerUpWeaponDamage AddDamage = player.gameObject.AddComponent<PowerUpWeaponDamage>();

                    // And call use of the vaiable
                    AddDamage.Use(player, outlastTime, powerUpValue, addPermanently);
                    //Destroy (AddDamage);

                }
                else if (!addPermanently)
                {
                    // Else get the component of the player...
                    PowerUpWeaponDamage AddDamage = player.gameObject.GetComponent<PowerUpWeaponDamage>();

                    // And call break and restart of the component
                    AddDamage.breakAndRestart();
                }
            }

            if (type == PowerUpEnum.weaponFireRate)
            {
                if (addPermanently || player.gameObject.GetComponent<PowerUpWeaponFireRate>() == null)
                {
                    PowerUpWeaponFireRate AddFireRate = player.gameObject.AddComponent<PowerUpWeaponFireRate>();
                    AddFireRate.Use(player, outlastTime, powerUpValue, addPermanently);
                    //Destroy (AddFireRate);
                }
                else if (!addPermanently)
                {
                    PowerUpWeaponFireRate AddFireRate = player.gameObject.GetComponent<PowerUpWeaponFireRate>();
                    AddFireRate.breakAndRestart();
                }
            }

            if (type == PowerUpEnum.lineCut)
            {
                // Only non permanent
                PowerUpCut lineCut = player.gameObject.GetComponent<PowerUpCut>();

                if (lineCut == null)
                {
                    lineCut = player.gameObject.AddComponent<PowerUpCut>();
                    lineCut.powerUpActiveTime = outlastTime;
                    lineCut.Use();
                }
                else
                {
                    lineCut.powerUpActiveTime = outlastTime;
                    lineCut.breakAndRestart();
                }
            }

            if (type == PowerUpEnum.mangoExplosion)
            {
                PowerUpMango mango = player.gameObject.AddComponent<PowerUpMango>();
                mango.SetExplosionProperties(explosionParticles, range);
                SoundManager.SoundManagerInstance.Play(clip, transform, 0.8f, 1, false);
                mango.Use();
            }
            //==================TEMPORARY_OR_PERMANENTLY_ADD_POWERUPS_end================================

            //=============PROTOTYPE_POWERUPS_START==================
            if (type == PowerUpEnum.addMaxHealth)
            {
                PowerUpAddMaxHealth AddHealth = player.gameObject.AddComponent<PowerUpAddMaxHealth>();
                AddHealth.Use(player, powerUpValue);
                //Destroy (AddHealth);
            }

            if (type == PowerUpEnum.addMaxEnergy)
            {
                PowerUpAddMaxEnergy AddEnergy = player.gameObject.AddComponent<PowerUpAddMaxEnergy>();
                AddEnergy.Use(player, powerUpValue);
                //Destroy (AddEnergy);
            }
            //=============PROTOTYPE_POWERUPS_END====================

            // Trigger Event.
            if (!singlePowerUp)
                CollectingPowerUp();

            // Set the player as parent gameobject for the colliding object
            GameObject playerParent = collider.gameObject;
            int i = 0;
            while (i < playerParent.transform.childCount)
            {
                GameObject childObject = playerParent.transform.GetChild(i).gameObject;
                if (childObject != null)
                {
                    if (childObject.GetComponent<PowerUpItem>() != null)
                    {
                        PowerUpItem powerUpComponent = childObject.GetComponent<PowerUpItem>();
                        powerUpComponent.StopAllCoroutines();
                        //childObject.SetActive(false);
                        Destroy(childObject);
                    }
                }
                i++;
            }
            // Let the object pend over the player
            GameObject pendingObject = Instantiate(gameObject, new Vector3(playerParent.transform.position.x, playerParent.transform.position.y + 2.25f, playerParent.transform.position.z), transform.rotation) as GameObject;

            // Get the sphere collider of the pending object
            SphereCollider pendingSphereCollider = pendingObject.GetComponent<SphereCollider>();

            // Deactivate the collider to avoid colliding
            pendingSphereCollider.enabled = false;

            // Scale it down to prepare it for tweening
            pendingObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Start tweening coroutine with an animation curve
            StartCoroutine(Tween(pendingObject.transform, new Vector3(1f, 1f, 1f), 0.5f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            // Start bck tweening coroutine
            StartCoroutine(TweenBack(pendingObject.transform, pendingTime));

            // Set the player to 
            pendingObject.transform.SetParent(playerParent.transform, true);
            //Debug.Log (pendingObject.transform.localScale);
            Destroy(pendingObject, pendingTime);

            // Particle
            if (pickUpParticles != null)
                Instantiate(pickUpParticles, transform.position, pickUpParticles.transform.rotation);

            // send event
            new Event(Event.TYPE.powerup).addWave().addCharacter(player.PlayerIdentifier.ToString("g")).addPowerup(type.ToString()).addPos(this.transform).addLevel().send();
            MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();

            // if the mesh renderer is in a childobject of the gameobject, then search all renderers of the child gameobject 
            // and store it into an Renderer array, then disable all
            if (meshRenderer == null)
            {
                Renderer[] meshRendererArray = GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in meshRendererArray)
                {
                    renderer.enabled = false;
                }
            }
            else
            {
                // deactivate mesh renderer to hide the collectible item until it is destroyed
                meshRenderer.enabled = false;
            }

            ParticleSystem particleSystem = transform.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particles in particleSystems)
                {
                    particles.emissionRate = 0.0F;
                }
            }
            else
            {
                // set particle emisiionrate to 0.0F to hide the collectible item until it is destroyed
                particleSystem.emissionRate = 0.0F;
            }

            // deactivate collider to prevent from multicasting the power up
            SphereCollider sphereCollider = transform.GetComponent<SphereCollider>();
            sphereCollider.enabled = false;

            DestroyCollectible(outlastTime);

        }
    }


    protected void CollectingPowerUp()
    {
        if (PowerUpCollected != null)
        {
            PowerUpCollected();
        }
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected void ResetValues()
    {
        PowerUpCollected = null;
    }

    /// <summary>
    /// Self implemented tweening method is needed to make it able to stop via StopAllCoroutines();
    /// </summary>
    /// <param name="transform">The actual game object as Transform.</param>
    /// <param name="target">Target scale.</param>
    /// <param name="duration">Duration of the tweening.</param>
    /// <param name="ease">Ease parameter.</param>
    /// <returns></returns>
    public IEnumerator Tween(Transform transform, Vector3 target, float duration, Easer ease)
    {
        float elapsed = 0;
        var start = transform.localScale;
        var range = target - start;
        while (elapsed < duration)
        {
            elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
            if (transform != null)
            {
                transform.localScale = start + range * ease(elapsed / duration);
                yield return 0;
            }
        }
        if (transform != null)
        {
            transform.localScale = target;
        }
    }

    /// <summary>
    /// Self implemented back tweening method
    /// </summary>
    /// <param name="pendingObject">The pending object as transform.</param>
    /// <param name="time">Time which should be awaited until the backtweening.</param>
    /// <returns></returns>
	public IEnumerator TweenBack(Transform pendingObject, float time)
    {
        float TWEENBACK_TIME = 0.3f;
        if (transform != null)
        {
            // Wait pending time - tweentime to let it fully tween back
            yield return new WaitForSeconds(time - TWEENBACK_TIME);
            if (pendingObject != null)
            {
                StartCoroutine(Tween(pendingObject.transform, new Vector3(0.05f, 0.05f, 0.05f), TWEENBACK_TIME, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
            }
        }
    }

    #endregion
}