using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the tweening of the sign arrows.
/// </summary>
public class ArrowSignScript : MonoBehaviour { 

    public GameObject[] arrows;
    int counter;
    Vector3[] originalSize;
    bool init = true;

    public float maxScaleFactor = 1.5f;
    public float minScaleFactor = 0.5f;
    public LeanTweenType tweenType;

    [SerializeField]
    protected float tweenTime;

    [SerializeField]
    protected float updateTime;

    public Color smallArrowColor;
    public Color bigArrowColor;

    // Use this for initialization
    void Start()
    {
        counter = 0;
        originalSize = new Vector3[arrows.Length];
        for (int i = 0; i < arrows.Length; i++)
        {
            originalSize[i] = arrows[i].transform.lossyScale;
        }
        InvokeRepeating("TweenArrows", 0f, updateTime);
    }

    private void TweenArrows()
    {
        int i = counter;
        LeanTween.cancel(arrows[i]); // cancel all tweens currently running on this GO

        // scale big
        LeanTween.scale(arrows[i], new Vector3(originalSize[i].x * maxScaleFactor, 1f, originalSize[i].z * maxScaleFactor), tweenTime).setEase(tweenType);
        LeanTween.value(arrows[i], smallArrowColor, bigArrowColor, tweenTime).setOnUpdate((Color value) => {
            Material m = arrows[i].GetComponent<Renderer>().material;
            m.SetColor("_Color", value);
            m.SetColor("_EmissionColor", value);
        }).setEase(tweenType);

        // scale small after delay
        LeanTween.scale(arrows[i], new Vector3(originalSize[i].x * minScaleFactor, 1f, originalSize[i].z * minScaleFactor), tweenTime).setDelay(tweenTime).setEase(tweenType);
        LeanTween.value(arrows[i], bigArrowColor, smallArrowColor, tweenTime).setDelay(tweenTime).setOnUpdate((Color value) => {
            Material m = arrows[i].GetComponent<Renderer>().material;
            m.SetColor("_Color", value);
            m.SetColor("_EmissionColor", value);
        }).setEase(tweenType);

        if (counter < arrows.Length-1)
            counter++;
        else
            counter = 0;
    }
}