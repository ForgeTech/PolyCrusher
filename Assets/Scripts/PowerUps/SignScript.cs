using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the resizing of the signs.
/// </summary>
public class SignScript : MonoBehaviour
{
    [Tooltip("The time for the scaling.")]
    [SerializeField]
    protected float scaleTime = 3f;

    Vector3 originalSize;

	// Use this for initialization
	void Start ()
    {
        originalSize = transform.localScale;
        InvokeRepeating("MakeScaleTransition", 0f, scaleTime + 0.05f);
	}

    private void MakeScaleTransition()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(transform.ScaleTo(new Vector3(originalSize.x, originalSize.y, originalSize.z), scaleTime, AnimCurveContainer.AnimCurve.shortUpscale.Evaluate));
    }
}