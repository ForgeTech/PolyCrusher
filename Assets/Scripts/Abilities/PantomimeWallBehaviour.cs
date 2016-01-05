using UnityEngine;
using System.Collections;

public class PantomimeWallBehaviour : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 7.5f;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine(WaitForDestroy());

        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        StartCoroutine(transform.ScaleTo(originalScale, 0.9f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
    }

    /// <summary>
    /// Waits for self destruction.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(activeTime);
        Destroy(this.gameObject);
    }
}
