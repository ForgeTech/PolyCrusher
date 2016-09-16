using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class FalloutBehaviour : MonoBehaviour {

    #region variables
    // The owner of the projectile.
    protected MonoBehaviour ownerScript;
    private SphereCollider sphereCollider;
    protected RumbleManager rumbleManager;
    private Vector3 currentScale;
    private List<BaseEnemy> detectedEnemies;
    private List<float> originalEnemyMovement;
    private MeshRenderer meshRenderer;
    private bool playAbilitySmoke = true;

    private WaitForSeconds waitForDestructions;

    [Header("Numeric settings")]
    [SerializeField]
    private float sphereRadius = 2.0f;

    [SerializeField]
    private float sphereRadiusBorder = 1.3f;

    [SerializeField]
    private float expandTime = 0.5f;

    [SerializeField]
    private float enemyIdleTime = 4.0f;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject smokePrefab;

    [SerializeField]
    private GameObject abilitySmokeStartPrefab;

    [SerializeField]
    private GameObject abilitySmokePrefab;

    [SerializeField]
    private GameObject falloutRing;
    #endregion

    // Instantiated ring
    private GameObject ring;

    #region properties
    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }

    /// <summary>
    /// Sets the rumble manager for accessing rumble functions
    /// </summary>
    public RumbleManager RumbleManager
    {
        set { rumbleManager = value; }
    }

    #endregion

    #region methods

    #region initialization
    private void Start ()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = sphereRadius;
        sphereCollider.enabled = true;

        detectedEnemies = new List<BaseEnemy>();
        originalEnemyMovement = new List<float>();

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        if (meshRenderer == null)
        {
            Debug.Log("Meshrenderer is null!");
        }

        waitForDestructions = new WaitForSeconds(enemyIdleTime);

        currentScale = new Vector3(0, 0, 0);

        Instantiate(abilitySmokeStartPrefab, transform.position, abilitySmokeStartPrefab.transform.rotation);

        StartTween();
	}
    #endregion

    #region startTween
    private void StartTween()
    {
        LeanTween.value(gameObject, 0.0f, sphereRadius, expandTime)
           .setOnUpdate((float radius) => {
               //sphereCollider.radius = radius;
               currentScale = Vector3.one * radius;
               transform.localScale = currentScale;
               if (radius > sphereRadiusBorder && playAbilitySmoke)
               {
                   playAbilitySmoke = false;
                   Instantiate(abilitySmokePrefab, transform.position, abilitySmokePrefab.transform.rotation);
               }
           })
           .setEase(LeanTweenType.easeInOutQuad)
           .setOnComplete(() => {
               CleanUp();
           });

        // Tween ring
        Vector3 spawnPosition = new Vector3(transform.position.x, -1f, transform.position.z);
        ring = Instantiate(falloutRing, spawnPosition, Quaternion.identity) as GameObject;
        ring.transform.localScale = new Vector3(sphereRadius * 2f, falloutRing.transform.localScale.y, sphereRadius * 2f);

        LeanTween.moveY(ring, 0f, expandTime).setEase(LeanTweenType.easeOutSine);
    }
    #endregion

    #region restoringMovement
    private void CleanUp()
    {
        transform.localScale = Vector3.zero;
        sphereCollider.enabled = false;
        StartCoroutine(Reset());
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(enemyIdleTime);
        LeanTween.moveY(ring, -1f, 0.4f).setEase(LeanTweenType.easeOutSine).setOnComplete(() => {
            Destroy(ring);
        });
        ResetEnemyMovementSpeed();
        detectedEnemies.Clear();
        originalEnemyMovement.Clear();
    }

    private void ResetEnemyMovementSpeed()
    {
        for(int i = 0; i < detectedEnemies.Count; i++)
        {
            if (detectedEnemies[i] != null)
            {
                detectedEnemies[i].MovementSpeed = originalEnemyMovement[i];
            }
        }
    }
    #endregion

    #region collisionDetection
    void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<MonoBehaviour>() is BaseEnemy && collider.tag == "Enemy")
        {
            BaseEnemy enemy = collider.GetComponent<BaseEnemy>();

            if(enemy!=null && enemy.MovementSpeed > 0.5f)
            {
                originalEnemyMovement.Add(enemy.MovementSpeed);
                detectedEnemies.Add(enemy);
                GameObject smoke = Instantiate(smokePrefab, enemy.transform.position, smokePrefab.transform.rotation) as GameObject;
                smoke.transform.SetParent(enemy.transform);

                StartCoroutine(WaitForDestruction(smoke));
                enemy.MovementSpeed = 0.0f;
            }
        }
    }
    #endregion

    private IEnumerator WaitForDestruction(GameObject gameObject)
    {
        yield return waitForDestructions;
        if(gameObject != null)
            Destroy(gameObject);
    }

    #region rumble
    private void Rumble()
    {
        if (rumbleManager != null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius, 1 << 8);

            for (int i = 0; i < hits.Length; i++)
            {
                rumbleManager.Rumble(hits[i].GetComponent<BasePlayer>().InputDevice, RumbleType.Timesphere);
            }
        }
    }
    #endregion

    #endregion
}
