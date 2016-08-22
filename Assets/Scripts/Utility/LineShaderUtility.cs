using UnityEngine;

public class LineShaderUtility : MonoBehaviour
{
    [SerializeField]
    public Vector3 startPosition;

    [SerializeField]
    public Vector3 endPosition;

    [SerializeField]
    public float width;

    [SerializeField]
    private Material lineMaterial;

    private LineRenderer lineRenderer;

	private void Start ()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(lineMaterial);
        lineRenderer.SetWidth(width, width);
        lineRenderer.useWorldSpace = true;
        lineRenderer.receiveShadows = false;
        lineRenderer.useLightProbes = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lineRenderer.SetVertexCount(2);
        
    }
	
	private void Update ()
    {
        lineRenderer.material.SetFloat("_LineLength", Vector3.Distance(startPosition, endPosition));
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
}