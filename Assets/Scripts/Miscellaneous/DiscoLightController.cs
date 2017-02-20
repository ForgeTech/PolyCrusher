using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class DiscoLightController : MonoBehaviour {

	[Header("Rotation")]
	[SerializeField]
	private bool useRotation = false;

	[SerializeField]
	private Vector3 endRotation;

	[SerializeField]
	private float rotationDuration;

	[SerializeField]
	private LeanTweenType leanTweenRotationType;

	[SerializeField]
	private float rotationWaitTime;

	[Header("Translation")]
	[SerializeField]
	private bool useTranslation = false;

	[SerializeField]
	private Vector3 endPosition;

	[SerializeField]
	private float translationDuration;

	[SerializeField]
	private LeanTweenType leanTweenTranslationType;

	[SerializeField]
	private float translationWaitTime;
	
	private Vector3 startPosition;
	private Vector3 startRotation;

	private bool translateForward = true;
	private bool rotateForward = true;

	private Light light;

	private WaitForSeconds tWait;
	private WaitForSeconds rWait;

	// Use this for initialization
	void Start () {
		light = GetComponent<Light>();

		tWait = new WaitForSeconds(translationWaitTime);
		rWait = new WaitForSeconds(rotationWaitTime);

		startPosition = transform.position;
		startRotation = transform.eulerAngles;	

		if(useTranslation){
			InitTranslationTween();
		}

		if(useRotation){
			InitRotationTween();
		}
	}

	private void InitTranslationTween(){
		Vector3 start = translateForward? startPosition : endPosition;
		Vector3 end = translateForward? endPosition : startPosition;

		 LeanTween.value(gameObject, start, end, translationDuration)
            .setOnUpdate((Vector3 newPos) => { transform.position = newPos; })
            .setEase(leanTweenTranslationType)
			.setOnComplete(()=>{translateForward = !translateForward; StartCoroutine(TranslationWait());});
	}


	private void InitRotationTween(){
		Vector3 start = rotateForward? startRotation : endRotation;
		Vector3 end = rotateForward? endRotation : startRotation;

		 LeanTween.value(gameObject, start, end, rotationDuration)
            .setOnUpdate((Vector3 newRotation) => { transform.eulerAngles = newRotation; })
            .setEase(leanTweenRotationType)
			.setOnComplete(()=>{rotateForward = !rotateForward; StartCoroutine(RotationWait());});
	}

	private IEnumerator TranslationWait(){
		yield return tWait;	
		InitTranslationTween();
	}

	private IEnumerator RotationWait(){
		yield return rWait;
		InitRotationTween();
	}
	
	void OnDrawGizmos(){
		Gizmos.color = Color.white;
		Gizmos.DrawCube(endPosition, new Vector3(0.25f, 0.25f, 0.25f));
	}
}
