using UnityEngine;
using System.Collections;

/// <summary>
/// A projectile which is represented as ray.
/// </summary>
public class RayProjectile : Projectile
{
    [Header("Ray parameters")]
    // The maximal length of the Ray.
    [SerializeField]
    protected float maxLength = 5f;

    // Number of vertices for the lightning look.
    [SerializeField]
    protected int numberOfVertices = 5;

    // The width of the rendered line.
    [SerializeField]
    protected float lineWidth = 0.1f;

    [SerializeField]
    protected int spiralVertices = 50;

    [SerializeField]
    protected float radius = 1.2f;

    [SerializeField]
    protected float circleFrequency = 2f;

    [SerializeField]
    protected float spiralRandomOffset = 0.1f;

    // The initial line width.
    private float initialLineWidth;

    // Fade Timer
    private float widthFadeTimer = 0f;

    // Help timer for the fade out.
    private float timerHelper = 0f;

    // The life time of the ray.
    [SerializeField]
    protected float rayLifeTime = 0.3f;

    // The offset of the vertices.
    [SerializeField]
    protected float lightningOffset = 0.3f;

    // The random offset of the ray.
    [SerializeField]
    protected float lengthOffset = 10f;

    [SerializeField]
    protected Material rayMaterial;

    [SerializeField]
    protected Material spiralMaterial;

    [Space(5)]
    [Header("Targeting values")]
    // The Target layer of the ray.
    [SerializeField]
    protected int targetLayer;

    // A reference to the line renderer.
    private LineRenderer lineRenderer;

    // Reference to the spiral line renderer.
    private LineRenderer spiralRenderer;
    private GameObject spiralGameObject;

    // Determines if the ray was already shot or not.
    private bool rayShot = false;

    /// <summary>
    /// Gets or sets the max length.
    /// </summary>
    public float MaxLength
    {
        get { return this.maxLength; }
        set { this.maxLength = value; }
    }

    /// <summary>
    /// Gets or sets the target layer of the ray.
    /// </summary>
    public int TargetLayer
    {
        get { return this.targetLayer; }
        set { this.targetLayer = value; }
    }

    /// <summary>
    /// MonoBehaviour Awake method.
    /// </summary>
    protected override void Awake()
    {
        InitLineRenderer();
    }

    void Start()
    {
        this.initialLineWidth = lineWidth;
    }

    /// <summary>
    /// The shoot mechanic of the RayProjectile.
    /// Call this to start a shot.
    /// </summary>
    protected override void Shoot()
    {
        if (!rayShot)
        {
            RaycastHit hitInfo;

            //Debug.Log("RayProjectile: Shoot()");

            
            // Boss shield layer
            if (targetLayer == 9 && Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << 16, QueryTriggerInteraction.Collide))
            {
                GenerateRay(hitInfo);

                BossShield shield = hitInfo.transform.GetComponent<BossShield>();
                if (shield != null)
                {
                    shield.CreateEnemyRay(hitInfo.point, Direction);
                }
            }
            else if (Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << targetLayer))
            {
                GenerateRay(hitInfo);
            }
            else
            {
                float distanceOffset = Random.Range(0f, lengthOffset);

                //Define first an last point.
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(numberOfVertices - 1, transform.position + Direction * (MaxLength + distanceOffset));
                CalculateLinePoints(MaxLength + distanceOffset);

                spiralRenderer.SetPosition(0, transform.position);
                spiralRenderer.SetPosition(spiralVertices - 1, transform.position + (Direction * (MaxLength + distanceOffset)));
                CalculateSpiralLinePoints(MaxLength + distanceOffset);
            }

            rayShot = true;
            //Destroy(gameObject, rayLifeTime);
            DestroyProjectile();
        }
    }

    /// <summary>
    /// Generates a ray and deals damage.
    /// </summary>
    protected void GenerateRay(RaycastHit hitInfo)
    {
        //Define first and last point.
        float distance = Vector3.Distance(transform.position, hitInfo.transform.position);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(numberOfVertices - 1, transform.position + (Direction * distance));
        CalculateLinePoints(distance);

        spiralRenderer.SetPosition(0, transform.position);
        spiralRenderer.SetPosition(spiralVertices - 1, transform.position + (Direction * distance));
        CalculateSpiralLinePoints(distance);

        // Check if the hit-object can take damage
        if (hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() is IDamageable)
        {
            (hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() as IDamageable).TakeDamage(Damage, this.OwnerScript);
            SpawnDeathParticle(hitInfo.transform.position);
        }
    }

    /// <summary>
    /// Shoots the ray with the specific params.
    /// </summary>
    /// <param name="direction">Direction of the ray.</param>
    /// <param name="maxLength">Max. length of the ray.</param>
    public void Shoot(Vector3 direction)
    {
        // Set params
        this.Direction = direction;

        // Start the shoot mechanism
        Shoot();
    }


    protected override void Update()
    {
        base.Update();

        SizeFadeOut();
    }

    /// <summary>
    /// Initializes the line renderer.
    /// </summary>
    private void InitLineRenderer()
    {
        GetComponent<MeshRenderer>().enabled = false;

        // Line renderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = rayMaterial;
        lineRenderer.SetWidth(lineWidth / 2f, lineWidth / 2f);
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.SetVertexCount(numberOfVertices);

        // Spiral renderer
        spiralGameObject = new GameObject("Spiral");
        spiralGameObject.transform.parent = transform;
        spiralGameObject.transform.rotation = transform.rotation;

        spiralRenderer = spiralGameObject.AddComponent<LineRenderer>();
        spiralRenderer.material = spiralMaterial;
        spiralRenderer.SetWidth(lineWidth / 2f, lineWidth / 2f);
        spiralRenderer.SetColors(Color.white, Color.white);
        spiralRenderer.SetVertexCount(spiralVertices);
    }

    /// <summary>
    /// Calculates the points of the line for a spiral.
    /// </summary>
    /// <param name="distance"></param>
    private void CalculateSpiralLinePoints(float distance)
    {
        if (numberOfVertices > 2)
        {
            float step = distance / spiralVertices;
            float stepAdd = step;

            for (int i = 0; i < spiralVertices - 2; i++)
            {
                float x =  Mathf.Sin(stepAdd * circleFrequency) * radius + Random.Range(-spiralRandomOffset, spiralRandomOffset);
                float y = Mathf.Cos(stepAdd * circleFrequency) * radius + Random.Range(-spiralRandomOffset, spiralRandomOffset);

                Quaternion rotation = Quaternion.LookRotation(Direction);
                Vector3 pos = new Vector3(x, y, 0);
                pos = rotation * pos;

                spiralRenderer.SetPosition(i + 1, transform.position + pos + Direction * stepAdd);
                stepAdd += step;
            }
        }
    }

    /// <summary>
    /// Calculate the points of the line renderer if thera are more than 2 points.
    /// </summary>
    private void CalculateLinePoints(float distance)
    {
        if (numberOfVertices > 2)
        {
            float step = distance / numberOfVertices;
            float stepAdd = step;
            Vector3 offset = Vector3.zero;

            for (int i = 0; i < numberOfVertices - 2; i++)
            {
                offset.x = Random.Range(-lightningOffset, lightningOffset);
                offset.z = Random.Range(-lightningOffset, lightningOffset);
                offset.y = Random.Range(-lightningOffset, lightningOffset);

                lineRenderer.SetPosition(i + 1, transform.position + offset +  Direction * stepAdd);
                stepAdd += step;
            }
        }
    }

    /// <summary>
    /// Spawns the death particles.
    /// </summary>
    protected override void SpawnDeathParticle(Vector3 position)
    {
        GameObject particle = Instantiate(deathParticlePrefab);
        particle.transform.position = position;
        particle.GetComponent<ParticleSystem>().Play();
    }

    /// <summary>
    /// Fade out mechanic.
    /// Makes the line smaller after the half of the life time passed.
    /// </summary>
    protected void SizeFadeOut()
    {
        if (timerHelper >= rayLifeTime / 2.0f)
        {
            float lerpWidth = Mathf.Lerp(initialLineWidth / 2.0f, 0f, widthFadeTimer / (rayLifeTime / 2.0f));
            lineRenderer.SetWidth(lerpWidth, lerpWidth);

            spiralRenderer.SetWidth(lerpWidth, lerpWidth);

            widthFadeTimer += Time.deltaTime;
        }

        timerHelper += Time.deltaTime;
    }

    /// <summary>
    /// Destroys the ray bullet after a the ray life time.
    /// </summary>
    protected override void DestroyProjectile()
    {
        Destroy(this.gameObject, rayLifeTime);
    }
}
