using UnityEngine;
using System.Collections;

public class BarrelRoll : MonoBehaviour {

	#region variables
	[SerializeField]
	private float startTime = 0.0f;
	
	[SerializeField]
	private float minResetTime = 4.0f;

	[SerializeField]
	private float maxResetTime = 6.0f;

	private float torqueModifier = 20.0f;	
	private Rigidbody rigid;
	private Vector3 startPosition;
	private Quaternion startRotation;
	#endregion

	// Use this for initialization
	void Start () {
		startPosition = transform.position;
		startRotation = transform.rotation;
		rigid = gameObject.GetComponent<Rigidbody>();
		StartCoroutine(Reset(startTime));
	}
	
	private void StartBarrelRoll(){
		rigid.angularVelocity=Vector3.zero;
		rigid.velocity = Vector3.zero;
	
		transform.position = startPosition;
		transform.rotation = startRotation;

		rigid.AddRelativeTorque(Vector3.forward*torqueModifier, ForceMode.Impulse);		
		StartCoroutine(Reset(Random.Range(minResetTime, maxResetTime)));
	}


	private IEnumerator Reset(float waitTime){
		yield return new WaitForSeconds(waitTime);
		StartBarrelRoll();	
	}
}
