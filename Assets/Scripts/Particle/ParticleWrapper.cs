using UnityEngine;
using System.Collections;

/// <summary>
/// Wraps a particle system to play it and destroy it.
/// </summary>
public class ParticleWrapper : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField]
    protected ParticleSystem particles;
    protected ParticleSystem p;

    protected virtual void Awake()
    {
        if (particles != null)
        {
            p = Instantiate(particles) as ParticleSystem;
            p.transform.position = transform.position;
            p.transform.parent = this.transform;


            if (p.playOnAwake)
                p.Play();
        }
        else
            p = null;
    }

	// Use this for initialization
	protected virtual void Start () 
    {
        if (p != null && particles.isPlaying)
            p.Play();

        // Destroy particles after it is finished.
        Destroy(this.gameObject, particles.duration);
	}
}
