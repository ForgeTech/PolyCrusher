using UnityEngine;
using System.Collections;

public class PantomimeWallBehaviour : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 7.5f;

    [SerializeField]
    private float collisionRadius = 3f;

    [SerializeField]
    private float radiusForwardOffset = 2f;

    [SerializeField]
    private float pushAwayForce = 10f;

    [SerializeField]
    private float backPositionTween = -5f;

    private void Start ()
    {
        NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
        StartCoroutine(WaitForDestroy());

        PushAwayEnemies();

        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        Vector3 backPosition = transform.forward * backPositionTween + transform.position;

        LeanTween.value(gameObject, backPosition, transform.position, 0.2f).setOnUpdate((Vector3 val) => {
            transform.position = val;
        }).setEase(LeanTweenType.easeOutSine);

        LeanTween.scale(gameObject, originalScale, 0.9f).setEase(AnimCurveContainer.AnimCurve.pingPong)
            .setOnComplete(() => {
                obstacle.enabled = true;
            });
    }

    private void PushAwayEnemies()
    {
        Collider[] collider = Physics.OverlapSphere(transform.position + transform.forward * radiusForwardOffset, collisionRadius, 1 << 9);
        for (int i = 0; i < collider.Length; i++)
        {
            Rigidbody rigid = collider[i].GetComponent<Rigidbody>();
            if (rigid != null)
                rigid.AddForce(transform.forward * pushAwayForce);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * radiusForwardOffset, collisionRadius);
    }
#endif
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