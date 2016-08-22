using UnityEngine;

public enum LineShaderType
{
    SineWave = 0,
    SawTooth = 1
}

public class LineShaderUtility : MonoBehaviour
{
    [Header("Line renderer settings")]
    [SerializeField]
    public Vector3 startPosition;

    [SerializeField]
    public Vector3 endPosition;

    [SerializeField]
    public float width;

    [SerializeField]
    private Material lineMaterial;

    [Header("Shader settings")]
    [SerializeField]
    public Color lineColor = Color.yellow;

    [SerializeField]
    public float amplitude = 0.4f;

    [SerializeField]
    public float frequeny = 5f;

    [SerializeField]
    public float smoothing = 0.1f;

    [SerializeField]
    public float speed = 100f;

    [SerializeField]
    public float colorStrength = 10f;

    [SerializeField]
    public LineShaderType functionType = LineShaderType.SineWave;

    private LineRenderer lineRenderer;

	void Start ()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(lineMaterial);
        lineRenderer.SetWidth(width, width);
        lineRenderer.useWorldSpace = true;
        lineRenderer.receiveShadows = false;
        lineRenderer.useLightProbes = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
	
	private void Update ()
    {
        lineRenderer.material.SetColor("_Color", lineColor);
        lineRenderer.material.SetFloat("_Frequency", frequeny);
        lineRenderer.material.SetFloat("_Amplitude", amplitude);
        lineRenderer.material.SetFloat("_Smoothing", smoothing);
        lineRenderer.material.SetFloat("_Speed", speed);
        lineRenderer.material.SetFloat("_ColorStrength", colorStrength);
        lineRenderer.material.SetInt("_FunctionType", (int) functionType);
        lineRenderer.material.SetFloat("_LineLength", Vector3.Distance(startPosition, endPosition));

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
}