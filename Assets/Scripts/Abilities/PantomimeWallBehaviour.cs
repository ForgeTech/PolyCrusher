using UnityEngine;
using System.Collections;

public class PantomimeWallBehaviour : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 7.5f;

    // Use this for initialization
    void Start ()
    {
        NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
        StartCoroutine(WaitForDestroy());

        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        LeanTween.scale(gameObject, originalScale, 0.9f).setEase(AnimCurveContainer.AnimCurve.pingPong)
            .setOnComplete(() => {
                obstacle.enabled = true;
            });
    }

    /// <summary>
    /// Waits for self destruction.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(activeTime);
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutSine);
        Destroy(this.gameObject, 0.55f);
    }
}
