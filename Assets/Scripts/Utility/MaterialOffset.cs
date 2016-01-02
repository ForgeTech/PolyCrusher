using UnityEngine;
using System.Collections;

/// <summary>
/// Offsets the texture
/// </summary>
public class MaterialOffset : MonoBehaviour 
{
    private Renderer r;

    float offset;
    
    [SerializeField]
    protected float offsetFactor = 0.5f;
    
    [SerializeField]
    protected bool enableScaling = true;

    Vector2 scaleOffset = new Vector2();

    void FixedUpdate()
    {
        CalculateOffset();
    }

    /// <summary>
    /// Calculates the offset.
    /// </summary>
    private void CalculateOffset()
    {
        r = GetComponent<Renderer>();
        offset = Time.time * offsetFactor;
        scaleOffset = new Vector2(Mathf.Cos(Time.time) * offsetFactor + 1, Mathf.Sin(Time.time) * offsetFactor + 1);

        float sinus = Mathf.Sin(offset);

        r.material.SetTextureOffset("_MainTex", new Vector2(offset, sinus));
        r.material.SetTextureOffset("_EmissionMap", new Vector2(offset, sinus));
        r.material.SetTextureOffset("_NormalMap", new Vector2(offset / 200f, sinus));

        if (enableScaling)
        {
            r.material.SetTextureScale("_MainTex", scaleOffset);
            r.material.SetTextureScale("_EmissionMap", scaleOffset);
        }
        r.material.SetTextureOffset("_DetailNormalMap", scaleOffset);
    }
}
