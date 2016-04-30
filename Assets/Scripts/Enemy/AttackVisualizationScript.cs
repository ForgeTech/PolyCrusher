using UnityEngine;
using System.Collections;

public class AttackVisualizationScript : MonoBehaviour {

    [SerializeField]
    protected float effectRadius;

    // TODO: Refactor to lerpTime
    [SerializeField]
    protected float lerpTime;

    // Current timer value.
    protected float currentTime;

    // The height of the tween animation.
    [SerializeField]
    protected float easeInHeight = 4f;

    // Start material color.
    [SerializeField]
    protected Material startColor;

    // End material color.
    [SerializeField]
    protected Material endColor;

    // Renderer reference.
    protected Renderer rend;

    // Particles of death.
    [SerializeField]
    protected GameObject explosionParticle;

    // Audioclip
    [SerializeField]
    protected AudioClip sound;

    // Pitch of the sound
    [Range(0f, 2f)]
    [SerializeField]
    protected float soundPitch = 0.5f;

    [Range(0f, 5f)]
    [SerializeField]
    protected float soundVolume = 0.3f;

    [SerializeField]
    protected bool animateOnStart = true;

    protected virtual void Start () {
        transform.localScale = Vector3.zero;
        rend = GetComponent<Renderer>();

        if (animateOnStart)
            FadeInAnimation();
    }
	
	protected virtual void Update () {
        WaitForFadeOut();
	}

    private void WaitForFadeOut() {
        if (currentTime >= lerpTime) {
            FadeOutHandler();
        }
        currentTime += Time.deltaTime;
    }

    /// <summary>
    /// Is called when the current time is overdue the lerp time.
    /// </summary>
    protected virtual void FadeOutHandler() {
        FadeOutAnimation();
        Destroy(gameObject, 0.28f);
    }

    protected virtual void FadeInAnimation() {
        Vector3 originalPos = transform.position;
        transform.position += new Vector3(0, easeInHeight, 0);

        StartCoroutine(transform.MoveTo(originalPos, lerpTime * 0.5f, Ease.CubeOut));
        StartCoroutine(transform.ScaleTo(new Vector3(effectRadius, effectRadius, 0.3f), lerpTime, Ease.CubeOut));
    }

    protected virtual void FadeOutAnimation() {
        StartCoroutine(transform.MoveTo(transform.position + new Vector3(0, easeInHeight, 0), 0.2f, Ease.CubeIn));
        StartCoroutine(transform.ScaleTo(Vector3.zero, 0.2f, Ease.CubeIn));
    }
}