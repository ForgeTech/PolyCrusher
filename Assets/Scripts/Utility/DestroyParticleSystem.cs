using UnityEngine;
using System.Collections;

/// <summary>
/// Destroys the particle system if it isn't active.
/// </summary>
public class DestroyParticleSystem : MonoBehaviour 
{
    void Start()
    {
        if (gameObject.GetComponent<ParticleSystem>() != null)
        {
            Destroy(gameObject, gameObject.GetComponent<ParticleSystem>().startLifetime);
        }
    }

    /*void Update()
    {
        if (gameObject.GetComponent<ParticleSystem>() != null)
        {
            if (gameObject.GetComponent<ParticleSystem>().isStopped)
                Destroy(this);
        }
    }*/
}
