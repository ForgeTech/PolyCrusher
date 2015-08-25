using UnityEngine;
using System.Collections;

/// <summary>
/// This script animates the vertices of the mesh to look like a water wave.
/// </summary>
public class WaterWaveDistortion : MonoBehaviour 
{
    // The speed of the animation.
    [SerializeField]
    protected float speed = 2.38f;

    // Tha scale of the animation.
    [SerializeField]
    protected float scale = 0.19f;

    // The strength of the noise.
    [SerializeField]
    protected float noiseStrength = 0.21f;

    // Walk factor of the noise.
    [SerializeField]
    protected float noiseWalk = 2.52f;

    // Original vertices.
    protected Vector3[] baseHeight;

    // Reference to the mesh component.
    Mesh modelMesh;

    // Specifies if the animation is running or not.
    [SerializeField]
    protected bool stop = false;

    // Renderer reference
    protected Renderer objRenderer;

    //The update time in seconds.
    [SerializeField]
    protected float updateTime = 0.03f;

    // Specifies if the update is allowed.
    protected bool updateAllowed = true;

    /// <summary>
    /// Gets or sets the stop value.
    /// </summary>
    public bool Stop
    {
        get { return this.stop; }
        set { this.stop = value; }
    }

	// Use this for initialization
	void Start ()
    {
        // Get the mesh.
        modelMesh = GetComponent<MeshFilter>().mesh;
        baseHeight = modelMesh.vertices;

        objRenderer = GetComponent<Renderer>();

        stop = true;
	}
	
	// Update is called once per frame
	void Update () 
    {
        //Debug.Log("WaterDistortion: Model - " + modelMesh + ", Stop: " + stop);
        if (updateAllowed && !stop)
        {
            CalculateWave();
            updateAllowed = false;
            StartCoroutine(WaitForNextUpdate());
        }
	}

    /// <summary>
    /// Calculates the wave.
    /// </summary>
    protected void CalculateWave()
    {
        Vector3[] vertices = new Vector3[baseHeight.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseHeight[i];
            v.y += Mathf.Sin(Time.time * speed + baseHeight[i].x + baseHeight[i].y + baseHeight[i].z) * scale;
            v.y += Mathf.PerlinNoise(baseHeight[i].x + noiseWalk, baseHeight[i].y + Mathf.Sin(Time.time * 0.1f)) * noiseStrength;
            vertices[i] = v;
        }

        modelMesh.vertices = vertices;
        modelMesh.RecalculateNormals();
    }

    /// <summary>
    /// Waits for the next update.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForNextUpdate()
    {
        yield return new WaitForSeconds(updateTime);
        updateAllowed = true;
    }

    /// <summary>
    /// Is called when the object became visible.
    /// </summary>
    protected virtual void OnBecameInvisible()
    {
        stop = true;
    }

    /// <summary>
    /// Is called when the object became invisible.
    /// </summary>
    protected virtual void OnBecameVisible()
    {
        stop = false;
    }
}
