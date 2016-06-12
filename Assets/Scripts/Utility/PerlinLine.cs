using UnityEngine;
using System.Collections;

/// <summary>
/// Utility script which draws a line between two points with legacy particles and a 
/// perlin noise based positioning.
/// This script needs a ParticleEmitter and a ParticleRenderer (both legacy) to operate.
/// </summary>
/// 
[RequireComponent(typeof(ParticleEmitter))]
[RequireComponent(typeof(ParticleRenderer))]
public class PerlinLine : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform startPosition;

    [SerializeField]
    private Transform endPosition;

    [Header("Particle settings")]
    [SerializeField]
    protected float particleSize = 0.6f;

    [SerializeField]
    [Tooltip("Particle quantity")]
    protected int particleQuantity = 100;

    [Header("Perlin noise settings")]
    [SerializeField]
    protected float speed = 1f;

    [SerializeField]
    protected float scale = 1f;

    [Header("Animation Settings")]
    [SerializeField]
    protected float rayAnimationSpeed = 2f;

    [SerializeField]
    protected AnimationCurve sizeCurve;

    #region Internal variables
    private Perlin noise = new Perlin();
    private float oneOverQuantity;      // 1 / particleQuantity
    private Particle[] particles;

    private ParticleEmitter pEmitter;

    // Noise variables
    private float timeX;
    private float timeY;
    private float timeZ;

    private Vector3 position;
    private Vector3 offset = Vector3.zero;
    private float currentLifeTime = 0f;
    #endregion

    void Awake ()
    {
        InitializeEmitter();
	}
	
	void Update ()
    {
        UpdateParticles();
        currentLifeTime += Time.deltaTime;
    }

    private void InitializeEmitter()
    {
        this.oneOverQuantity = 1f / particleQuantity;

        pEmitter = GetComponent<ParticleEmitter>();
        pEmitter.emit = false;
        pEmitter.Emit(particleQuantity);

        particles = pEmitter.particles;
    }

    private void UpdateParticles()
    {
        if (startPosition != null && endPosition != null)
        {
            UpdateTime();
            CalculateParticlePosition();

            // Set calculated particle positions
            pEmitter.particles = particles;
        }
    }

    private void CalculateParticlePosition()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            position = Vector3.Lerp(startPosition.position, endPosition.position, oneOverQuantity * (float) i);

            // Use Set() to avoid 'new Vector()'
            offset.Set(noise.Noise(timeX + position.x, timeX + position.y, timeX + position.z),
                                        noise.Noise(timeY + position.x, timeY + position.y, timeY + position.z),
                                        noise.Noise(timeZ + position.x, timeZ + position.y, timeZ + position.z));

            position += (offset * scale * ((float)i * oneOverQuantity));

            particles[i].position = position;
            particles[i].color = Color.white;

            // oneOverQuantity * i: The lightning ray matches at each position the size of the graph
            // currentLifeTime * rayAnimationSpeed - 0.5f: Starts with -0.5 -> So it starts in the middle of the graph and then the whole graph is shifted
            //                                                                 This causes the pulse effect.
            particles[i].size = particleSize * sizeCurve.Evaluate(oneOverQuantity * (float)i - (currentLifeTime * -rayAnimationSpeed - 0.5f));
        }
    }

    private void UpdateTime()
    {
        timeX = Time.time * speed * 0.1365143f;
        timeY = Time.time * speed * 1.21688f;
        timeZ = Time.time * speed * 2.5564f;
    }
}
