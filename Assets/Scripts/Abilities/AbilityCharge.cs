using UnityEngine;
using System.Collections;


[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(BoxCollider))]
public class AbilityCharge : Ability {

    //---VARIABLES START

    //---public

    [Space(5)]
    [Header("Charge settings:")]
    //acceleration value (linear)
    public float chargeAcceleration = 1000f;
    
    //how far the charger sprints 
    public float chargeDistance = 5;
    
    //how far the enemies are pushed back by the explosion
    public float explosionForce = 150;

    // The radius of the explosion.
    public float explosionRadius = 10f;

    // The damage of the explosion.
    public int explosionDamage = 50;
    //---private

    //array that holds the rigidbodies of every player that is near the charger while charging
    private Rigidbody[] friends;
    
    //array that holds all players
    private Rigidbody[] players;

    //whether th charge is happening
    private bool charging;
    
    //if activated, the sphere collider detects nearby allies
    private bool detectFriends;
   
    //activated after the charge for the final explosion
    private bool explosion;

    //activated as last phase of the charge, for restoring all settings 
    private bool cleanUp;
    
    //counter that holds the currently free position in the friends array
    private int currentFriend;
    
    //the charging speed, that is incremented over time using the acceleration
    private float chargeSpeed;

    //holds the forward vector of the parent gameobject
    private Vector3 chargeDirection;

    //when the charging is activated, the start position is stored (used for calculating the already charged distance)
    private Vector3 oldPosition;

    //rigidbody object of the parent gameobject
    private Rigidbody player;

    [Header("Particle")]

    // Particles for the explosion
    [SerializeField]
    protected GameObject explosionParticle;

    // Particles for the charge
    [SerializeField]
    protected GameObject chargeParticle;

    // Instantiatet charge particles
    private GameObject instantiatedChargeParticle;
    //---VARIABLES END

    void Awake()
    {
        BasePlayer.PlayerDied += UpdatePlayerStatus;
        BasePlayer.PlayerSpawned += UpdatePlayerStatus;
    }


    protected override void Start()
    {
        base.Start();

        //initialize the friends array with 3, there can be no more friends^^
        friends = new Rigidbody[3];
        players = new Rigidbody[4];
        currentFriend = 0;        
        useIsAllowed = true;
        player = GetComponentInParent<Rigidbody>();        
    }


    void FixedUpdate()
    {      
        if (charging)
        {
            base.Use();
            //if the charged distance is smaller than the max allowed distance, continue to apply force 
            if (Vector3.Distance(transform.position, oldPosition) < chargeDistance)
            {
                //calculate new speed
                chargeSpeed += chargeAcceleration;

                //add force to charger and all allies stored in the friends array

                //readjust the constraints for the charging process (so the player wont fall or act weirdly while colliding with other rigidbodies)
                player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                //while charging the mass is increased, for a better overal experience^^
                player.mass = 10.0f;

                player.AddForce(chargeDirection * chargeSpeed);

                for (int i = 0; i < friends.Length; i++)
                {
                    if (friends[i] != null)
                    {
                        if (friends[i].mass != 10.0f)
                        {
                            friends[i].mass = 10.0f;                            
                        }
                        
                        friends[i].AddForce(chargeDirection * chargeSpeed);
                    }
                }
            }           
        }

        if (detectFriends)
        {
            currentFriend = 0;
        }


        if (cleanUp && !charging)
        {
            cleanUp = false;
            player.mass = 0.2f;
           
            for (int i = 0; i < friends.Length; i++)
            {
                if (friends[i] != null)
                {
                    friends[i].mass = 0.2f;                   
                }
            }

            //clear the friends array
            friends = new Rigidbody[3];
            currentFriend = 0;

            //set the radius back to normal and deactivate colliders
            GetComponent<SphereCollider>().radius = 20.0f;
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;        
            
        }
    }


    public override void Use()
    {   
        if (useIsAllowed)
        {
            useIsAllowed = false;
            charging = true;
            StartCoroutine(WaitForNextAbility());

            //save the current position of the charger for distance calculations later on
            oldPosition = transform.position;
            //save the current forward vector, in order to have the charger sprint in a fixed direction
            chargeDirection = transform.forward;           

            //enable the box collider for pushing away enemies
            GetComponent<BoxCollider>().enabled = true;
            //enable the sphere collider for detecting allies and enemies
            GetComponent<SphereCollider>().enabled = true;
            detectFriends = true;
            //animator.SetBool("BeginCharge", true);

            // Particles
            if (chargeParticle != null)
            {
                instantiatedChargeParticle = Instantiate(chargeParticle, transform.position, chargeParticle.transform.rotation) as GameObject;
                instantiatedChargeParticle.transform.parent = transform;
            }
            
            StartCoroutine(ChargerTimer());            
        }
    }

    /// <summary>
    /// Gets collidable objects in range.
    /// </summary>
    protected virtual Collider[] GetCollidersInRange()
    {
        // All Enemy hits.
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        return hits;
    }

    /// <summary>
    /// Deals the damage to all enemies in range.
    /// </summary>
    protected virtual void DealDamage()
    {
        Collider[] hits = GetCollidersInRange();

        foreach (Collider c in hits)
        {
            if (c.gameObject.GetComponent<MonoBehaviour>() is BaseEnemy)
            {
                BaseEnemy e = c.gameObject.GetComponent<MonoBehaviour>() as BaseEnemy;
                e.TakeDamage(explosionDamage, this.OwnerScript);
            }
        }

    }

    //limits the charging duration
    private IEnumerator ChargerTimer()
    {
        yield return new WaitForSeconds(0.1f);
        detectFriends = false;
        yield return new WaitForSeconds(0.1f);

        for(int i = 0; i < players.Length; i++)
        {
            if(players[i]!= null && players[i].mass!=0.2f)
            {
                players[i].mass = 0.2f;

            }
        }
        
        charging = false;
        StartCoroutine(ExplosionTimer());        
    }

    //executes the explosion
    private IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(0.2f);         
        
        GetComponent<SphereCollider>().radius = 50.0f;        
        
        chargeSpeed = 0.0f;
     
        explosion = true;

        // Deal damage.
        DealDamage();      

        // Detach smoke
        instantiatedChargeParticle.transform.parent = null;

        //Particle
        if (explosionParticle != null)
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);

        // Camera shake
        CameraManager.CameraReference.ShakeOnce();
       
        StartCoroutine(CleanUp());
    }

    private IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(0.2f);
        explosion = false;
        cleanUp = true;
        yield return new WaitForSeconds(0.2f);
    }


    void OnTriggerStay(Collider coll)
    {
        //detects nearby allies and adds them to the friends array
        if (detectFriends)
        {
            if (coll.tag == "Player")
            {
                friends[currentFriend] = coll.GetComponent<Rigidbody>();
                currentFriend++;               
            }
        }

        //detects nearby enemies and pushs them away from the charger
        if (explosion)
        {
            if (coll.tag == "Enemy")
            {                
                coll.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius);
                coll.GetComponent<BaseEnemy>().TakeDamage(0, this.OwnerScript);
            }
        }
    }

    private void UpdatePlayerStatus()
    {
        players = new Rigidbody[4];
        GameObject[] go = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < go.Length; i++)
        {
            players[i] = go[i].GetComponent<Rigidbody>();


        }

    }


    private void UpdatePlayerStatus(BasePlayer player)
    {
        UpdatePlayerStatus();

    }
}
