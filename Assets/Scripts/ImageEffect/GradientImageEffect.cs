using UnityEngine;

[ExecuteInEditMode]
public class GradientImageEffect : MonoBehaviour
{
    [SerializeField]
    protected Shader currentShader;

    [SerializeField]
    protected Texture gradientMap;

    [SerializeField]
    [Range(0f, 1f)]
    protected float gradientStrength = 1f;

    protected Material currentMaterial;

    #region Properties
    public Material material
    {
        get
        {
            if (currentMaterial == null)
            {
                currentMaterial = new Material(currentShader);
                currentMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return currentMaterial;
        }
    }
    #endregion

    private void Start ()
    {
        if (!SystemInfo.supportsImageEffects)
            enabled = false;
        else if (!currentShader && !currentShader.isSupported)
            enabled = false;
	}


    private void Update ()
    {
        gradientStrength = Mathf.Clamp(gradientStrength, 0f, 1f);
	}

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (currentShader != null)
        {
            // Set material properties
            material.SetTexture("_GradientMap", gradientMap);
            material.SetFloat("_GradientStrength", gradientStrength);
            Graphics.Blit(source, destination, material);
        }
        else
            Graphics.Blit(source, destination);
    }

    private void OnDisable()
    {
        if (currentMaterial)
            DestroyImmediate(currentMaterial);
    }
}
