using UnityEngine;
using System.Collections;

public class ParticleExplosionWrapper : ParticleWrapper 
{
    [Header("Light color")]
    [SerializeField]
    protected Color lightColor = Color.yellow;

    [Header("Fade time of the light")]
    [SerializeField]
    protected float lightFadeTime = 0.5f;

    // Current fade time of the light.
    private float currentLightFadeTime = 0f;

    // Light reference.
    protected GameObject explosionLight;

    // Light component reference.
    protected Light lightComponent;

    // The initial light intensity.
    private float initialLightIntensity;


    protected override void Awake()
    {
        base.Awake();
    }

	protected override void Start () 
    {
        base.Start();

        explosionLight = new GameObject("Explosion Light");
        lightComponent = explosionLight.AddComponent<Light>();
        lightComponent.bounceIntensity = 0f;
        lightComponent.range = 6f;
        lightComponent.intensity = 7.0f;
        lightComponent.color = lightColor;
        explosionLight.transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
        explosionLight.transform.parent = transform;

        initialLightIntensity = lightComponent.intensity;
	}

    void Update()
    {
        FadeLight();
    }

    /// <summary>
    /// Fades the light from it's start intensity to zero over the light fade time.
    /// </summary>
    protected void FadeLight()
    {
        lightComponent.intensity = Mathf.Lerp(initialLightIntensity, 0.0f, currentLightFadeTime / lightFadeTime);
        currentLightFadeTime += Time.deltaTime;
    }
}