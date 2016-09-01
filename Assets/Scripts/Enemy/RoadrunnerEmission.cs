using UnityEngine;

[RequireComponent(typeof(RoadrunnerEnemy))]
public class RoadrunnerEmission : MonoBehaviour
{
    [SerializeField]
    private float blinkTime = 0.5f;

    private Material roadrunnerMaterial;

	private void Start ()
    {
        roadrunnerMaterial = gameObject.transform.GetComponentInChildren<Renderer>().material;
        roadrunnerMaterial.SetFloat("_Emission", 1f);
        TweenStandardEmission();

        BaseEnemy e = GetComponent<BaseEnemy>();
        e.AttackAhead += DoAttackTween;
        e.AttackCanceled += CancelAttackTween;
	}

    private void TweenStandardEmission()
    {
        LeanTween.value(gameObject, 0f, 1f, blinkTime).setEase(LeanTweenType.easeOutSine).setLoopPingPong()
            .setOnUpdate((float val) => {
                roadrunnerMaterial.SetFloat("_Emission", val);
            });
    }

    private void DoAttackTween(float currentAttackTime, float maxAttackTime)
    {
        Debug.Log("<b>Death emission tween.</b>");
        LeanTween.cancel(gameObject);
        float tweenTime = Mathf.Abs(maxAttackTime - currentAttackTime);

        LeanTween.value(gameObject, 0f, 10f, tweenTime).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) => {
                roadrunnerMaterial.SetFloat("_Emission", val);
            });
    }

    private void CancelAttackTween()
    {
        LeanTween.cancel(gameObject);
        TweenStandardEmission();
    }
}
