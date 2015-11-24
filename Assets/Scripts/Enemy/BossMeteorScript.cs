using UnityEngine;
using System.Collections;

public class BossMeteorScript : MonoBehaviour
{
    [Header("Meteor spawn:")]

    [SerializeField]
    [Tooltip("Prefab of the meteor bullet.")]
    protected GameObject meteorBullet;

    [SerializeField]
    [Tooltip("Spawn area radius of the meteors.")]
    protected float meteorSpawnRadius = 10f;

    [SerializeField]
    [Tooltip("Spawn time of the meteors, after this time the gameobject destroys itself.")]
    protected float meteorSpawnTime = 3f;

    [SerializeField]
    [Tooltip("Spawn interval between the meteorits. (In Seconds)")]
    protected float meteorSpawnIntervall = 0.4f;

    [SerializeField]
    [Tooltip("Random meteor height spread.")]
    protected float meteorHeightSpread = 7f;

    [SerializeField]
    [Tooltip("The meteors are traveling downwards, so the offset makes them fly diagonal.")]
    protected Vector3 destinationOffset;

    [Space(4)]
    [Header("Debug:")]
    [SerializeField]
    [Tooltip("Debug Mesh")]
    protected Mesh debugEditorMesh;

    // The owner of this script.
    protected MonoBehaviour ownerScript;

    protected bool meteorScriptInitialized;

    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }

    // Determines if the spawn of an meteor is allowed or not.
    private bool spawnAllowed;

	// Use this for initialization
	void Start ()
    {
        spawnAllowed = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(meteorScriptInitialized)
            SpawnMeteor();
	}

    /// <summary>
    /// Spawns the meteors.
    /// </summary>
    protected void SpawnMeteor()
    {
        // Only spawn if allowed.
        if (spawnAllowed)
        {
            // Calculate spawn point.
            Vector3 spawnPoint = CalculateSpawnPoint();
            
            // Create bullet.
            SpawnBullet(spawnPoint);

            spawnAllowed = false;
            StartCoroutine(EnableMeteorSpawn());
        }
    }

    /// <summary>
    /// Calculates the spawn point of one meteorit.
    /// </summary>
    /// <returns>Spawn point as Vector3</returns>
    protected Vector3 CalculateSpawnPoint()
    {
        // Random point in the spawn radius.
        Vector2 randomCirclePoint = (Random.insideUnitCircle * meteorSpawnRadius) + new Vector2(transform.position.x, transform.position.z);

        // Random height in the height spread range.
        float randomHeight = Random.Range(transform.position.y, transform.position.y + meteorHeightSpread);

        return new Vector3(randomCirclePoint.x, randomHeight, randomCirclePoint.y);
    }

    /// <summary>
    /// Spawns the bullet at a specific spawn point.
    /// </summary>
    /// <param name="spawnPoint">Spawn point of the meteor.</param>
    protected void SpawnBullet(Vector3 spawnPoint)
    {
        if (meteorBullet != null)
        {
            //GameObject g = GameObject.Instantiate(meteorBullet);
            GameObject g = ObjectsPool.Spawn(meteorBullet, Vector3.zero, meteorBullet.transform.rotation);
            MeteorBullet bullet;

            if (g.GetComponent<MonoBehaviour>() is MeteorBullet)
            {
                bullet = g.GetComponent<MeteorBullet>();
                bullet.OwnerScript = ownerScript;
                bullet.name = "MeteorBullet";

                bullet.transform.position = spawnPoint;
                Vector3 direction = Vector3.down + destinationOffset;

                bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

                bullet.Shoot(direction, bullet.BulletSpeed);
            }
        }
    }

    public void InitializeScript(BossEnemy owner)
    {
        this.OwnerScript = owner;
        this.meteorScriptInitialized = true;
        Destroy(this.gameObject, meteorSpawnTime);
    }

    /// <summary>
    /// Enables the meteor spawn after the spawn intervall.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator EnableMeteorSpawn()
    {
        yield return new WaitForSeconds(meteorSpawnIntervall);
        spawnAllowed = true;
    }

    /// <summary>
    /// Draws the debug ring.
    /// </summary>
    void OnDrawGizmos()
    {
        // Normal debug ring
        Gizmos.DrawMesh(debugEditorMesh, transform.position, Quaternion.Euler(-90, 0, 0), new Vector3(meteorSpawnRadius, meteorSpawnRadius, 1));

        // Height debug ring
        Gizmos.DrawMesh(debugEditorMesh, transform.position + new Vector3(0, meteorHeightSpread, 0), Quaternion.Euler(-90, 0, 0), new Vector3(meteorSpawnRadius, meteorSpawnRadius, 1));
    }
}
