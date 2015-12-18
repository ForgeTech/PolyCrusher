using UnityEngine;
using System.Collections;

public class SpecialPowerUp : MonoBehaviour {

 
    
    // Lifetime of the pending object
    [SerializeField]
    private float pendingTime = 2f;

    // Add some spacing in the Unity Inspector
    [Space(5)]
    // Add a header above some fields in the Unity Inspector
    [Header("Particles")]
    [SerializeField]
    // Particles which are staying in the air after picking up
    protected GameObject pickUpParticles;

    // Event handler for the collectible collected.
    public static event PowerUpCollectedHandler PowerUpCollected;


   
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
           
            // Trigger Event.
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
            new Event(Event.TYPE.powerup).addWave().addCharacter(player.PlayerName).addPos(this.transform).addLevel().send();
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

            Destroy(this);

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
}
