using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageShaking : MonoBehaviour
{
    [SerializeField]
    private float minSpeed = 4f;

    [SerializeField]
    private float maxSpeed = 8f;

    [SerializeField]
    private float shakeScale = 10f;

    private Image img;

	void Start ()
    {
        float speed = Random.Range(minSpeed, maxSpeed);

        img = GetComponent<Image>();
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        randomDirection.Normalize();

        LeanTween.value(img.gameObject, Vector2.zero, randomDirection * shakeScale, speed)
            .setEase(LeanTweenType.easeShake)
            .setOnUpdate((Vector2 val) => {
                img.rectTransform.anchoredPosition += val;
            })
            .setLoopPingPong();
	}
}