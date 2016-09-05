using UnityEngine;
using System.Collections;

public class LightSabreScript : MonoBehaviour {


    [SerializeField]
    private float sabreRadius;

    [SerializeField]
    private LineShaderUtility lineShader;

    [SerializeField]
    private float sabreAnimationTime = 2.0f;

    [SerializeField]
    private float heightOffset = 1.0f;

    [SerializeField]
    private float powerUpDuration = 12.0f;

    [SerializeField]
    private int bossCuttingDamage = 250;

    [SerializeField]
    private GameObject laserParticles;


    private Vector3 sabreStartPosition = new Vector3();
    private Vector3 sabreEndPosition = new Vector3();

    private Vector3 middlePosition = new Vector3();

    private Vector3 sabreStartWorldLocation = new Vector3();
    private Vector3 sabreEndWorldLocation = new Vector3();

    private Vector3 offsetVector = new Vector3();

	// Use this for initialization
	void Start () {
        offsetVector.y = heightOffset;
        CalculateSabrePositions();
        LightSabreTween(true);
        StartCoroutine(Deactivate(powerUpDuration));
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit[] hits;

        sabreStartWorldLocation = transform.TransformPoint(sabreStartPosition);
        sabreEndWorldLocation = transform.TransformPoint(sabreEndPosition);

        hits = Physics.RaycastAll(new Ray(sabreEndWorldLocation, Vector3.Normalize(sabreStartWorldLocation - sabreEndWorldLocation)), Vector3.Distance(sabreEndWorldLocation, sabreStartWorldLocation), (1 << 9));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.GetComponent<MonoBehaviour>())
            {
                MonoBehaviour gotHit = hit.transform.GetComponent<MonoBehaviour>();
                if (gotHit is BaseEnemy)
                {
                    BaseEnemy enemy = hit.transform.GetComponent<BaseEnemy>();
                    if (gotHit is BossEnemy)
                    {
                        Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation), 2);
                        enemy.TakeDamage(bossCuttingDamage, this);
                    }
                    else
                    {
                        enemy.InstantKill(this);
                        enemy.gameObject.AddComponent<CutUpMesh>();
                        Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation), 2);
                    }
                }
            }
        }
    }


    private IEnumerator Deactivate(float duration)
    {
        yield return new WaitForSeconds(duration);
        LightSabreTween(false);
    }
    


    private void CalculateSabrePositions()
    {
        sabreStartPosition = transform.InverseTransformPoint(transform.parent.position) + (transform.parent.right * sabreRadius) + offsetVector;
        sabreEndPosition = transform.InverseTransformPoint(transform.parent.position) - (transform.parent.right * sabreRadius) + offsetVector;
        middlePosition = transform.InverseTransformPoint(transform.parent.position) + offsetVector;
    }

    #region tweens
    private void LightSabreTween(bool animateIn)
    {
        Vector3 first = middlePosition;
        Vector3 second = sabreStartPosition;

        if (!animateIn)
        {
            first = sabreStartPosition;
            second = middlePosition;
        }


        LeanTween.value(gameObject, first, second, sabreAnimationTime).setOnUpdate(
            (Vector3 pos) => { lineShader.startPosition = pos; }
        ).setEase(LeanTweenType.easeInQuad);


        first = middlePosition;
        second = sabreEndPosition;

        if (!animateIn)
        {
            first = sabreEndPosition;
            second = middlePosition;
        }


        LeanTween.value(gameObject, first, second, sabreAnimationTime).setOnUpdate(
           (Vector3 pos) => { lineShader.endPosition = pos; }
        ).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { if (!animateIn) { Destroy(gameObject); } });


        LeanTween.value(gameObject, 2.0f, 0.8f, sabreAnimationTime).setOnUpdate(
           (float lerp) => { lineShader.width = lerp; }
        ).setEase(LeanTweenType.easeInQuad);

        if (animateIn)
        {
            LeanTween.value(gameObject, 30.0f, 5.0f, sabreAnimationTime).setOnUpdate(
            (float lerp) => { lineShader.colorStrength = lerp; }
            ).setEase(LeanTweenType.easeInQuad);
        }
        

    }

    #endregion
}
