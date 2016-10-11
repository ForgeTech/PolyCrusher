using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class UpDownLerp : MonoBehaviour
{
    [SerializeField]
    private float offset = 10f;

    [SerializeField]
    private float time = 0.8f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

	private void Start ()
    {
        RectTransform rect = GetComponent<RectTransform>();
        LeanTween.moveX(rect, rect.anchoredPosition.x + offset, time).setLoopPingPong().setEase(easeType);
        LeanTween.moveY(rect, rect.anchoredPosition.y + offset * 0.8f, time * 0.8f).setLoopPingPong().setEase(easeType);
    }
}
