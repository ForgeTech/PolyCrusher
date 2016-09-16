using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 30f;

    [SerializeField]
    private float powerUpDamageRotationSpeedIncrease = 60f;

    [SerializeField]
    private float shakeAmount = 0.4f;

    [SerializeField]
    private float shakeTime = 0.2f;

    private float currentYRotation = 0f;
    private Quaternion arrowRotation;
    private Quaternion originalRotation;
    private BasePlayer player;
    private Material arrowMaterial;
    private Color originalEmissionColor;
    private Color originalColor;
    private MeshRenderer meshRenderer;

    private void Start ()
    {
        originalRotation = transform.rotation;
        arrowRotation = originalRotation * Quaternion.Euler(Vector3.up * Random.Range(0, 360));

        arrowMaterial = GetComponent<Renderer>().material;
        meshRenderer = GetComponent<MeshRenderer>();
        originalEmissionColor = arrowMaterial.GetColor("_EmissionColor");
        originalColor = arrowMaterial.GetColor("_Color");

        player = transform.root.GetComponent<BasePlayer>();

        player.DamageTaken += DoHealthDamageTween;
        player.PlayerWeapon.DamageIncreased += IncreaseRotationSpeed;
    }

    private void OnDisable()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void OnEnable()
    {
        if(meshRenderer != null)
            meshRenderer.enabled = true;
    }

	private void Update ()
    {
        ResetParentRotation();
        RotateYAxis();
	}

    private void IncreaseRotationSpeed()
    {
        rotationSpeed += powerUpDamageRotationSpeedIncrease;
    }

    private void DoHealthDamageTween(int damage)
    {
        if (!LeanTween.isTweening(gameObject))
        {
            LeanTween.moveLocalZ(gameObject, shakeAmount, shakeTime).setEase(LeanTweenType.easeShake);
            LeanTween.value(gameObject, originalEmissionColor, Color.black, shakeTime).setEase(LeanTweenType.easeShake)
                .setOnUpdate((Color val) => {
                    arrowMaterial.SetColor("_EmissionColor", val);
                });
            LeanTween.value(gameObject, originalColor, Color.white, shakeTime).setEase(LeanTweenType.easeShake)
                .setOnUpdate((Color val) => {
                    arrowMaterial.SetColor("_Color", val);
                });
        }
    }

    private void ResetParentRotation ()
    {
        transform.rotation = originalRotation;
    }

    private void RotateYAxis ()
    {
        arrowRotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * rotationSpeed);
        transform.rotation = arrowRotation;
    }
}
