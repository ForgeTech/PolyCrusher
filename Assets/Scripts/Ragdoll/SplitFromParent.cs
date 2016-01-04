using UnityEngine;
using System.Collections;

public class SplitFromParent : MonoBehaviour {
	

	public void Split(float time)
    {
        transform.parent = null;
        //StartCoroutine(transform.ScaleFrom(new Vector3(0.4f, 0.4f, 0.4f), time, AnimCurveContainer.AnimCurve.downscale.Evaluate));
        Destroy(gameObject, time);
    }
}
