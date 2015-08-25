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

    [Space(5)]
    [Header("Targeting values")]
    // The Target layer of the ray.
    [SerializeField]
    protected int targetLayer;

    // A reference to the line renderer.
    private LineRenderer lineRenderer;

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
    void Awake()
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

            // Check if there was a hit.
            if (Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << targetLayer))
            {
                //Debug.Log("RayProjectile: Hit!");

                //Define first and last point.
                
                float distance = Vector3.Distance(transform.position, hitInfo.transform.position);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(numberOfVertices - 1, transform.position + (Direction * distance));
                CalculateLinePoints(distance);

                // Check if the hit-object can take damage
                if (hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() is IDamageable)
                {
                    (hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() as IDamageable).TakeDamage(Damage, this.OwnerScript);
                }

                SpawnDeathParticle(hitInfo.transform.position);
            }
            else
            {
                float distanceOffset = Random.Range(0f, lengthOffset);

                //Define first an last point.
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(numberOfVertices - 1, transform.position + Direction * (MaxLength + distanceOffset));
                CalculateLinePoints(MaxLength + distanceOffset);
            }

            rayShot = true;
            Destroy(gameObject, rayLifeTime);
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

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Resources.Load<Material>("Material/Bullet/RayMaterial"));
        lineRenderer.SetWidth(lineWidth / 2f, lineWidth / 2f);
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.SetVertexCount(numberOfVertices);
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


            widthFadeTimer += Time.deltaTime;
        }

        timerHelper += Time.deltaTime;
    }
}
