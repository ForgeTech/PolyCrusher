using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine.UI;

/// <summary>
/// Level exit delegate.
/// </summary>
public delegate void LevelExitDelegate();

public class LevelEndManager : MonoBehaviour
{
    #region Inspector variables
    [SerializeField]
    private string levelToLoad = "";

    [SerializeField]
    [Tooltip("Wait time in seconds.")]
    private float waitTimeBeforeLevelLoad = 1.0f;

    [SerializeField]
    private Image crushedCircle;

    [SerializeField]
    private Text crushedText;

    [Header("Tweening setting")]
    [SerializeField]
    private float tweenTime = 0.5f;

    [SerializeField]
    private float crushedTextTweenTime = 0.1f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [Header("Camera effect setting")]
    [SerializeField]
    private float destinationIntensity = 0.7f;

    [SerializeField]
    private float destinationGreenIntensity = 0.3f;

    [SerializeField]
    private AudioClip wooshSound;

    [SerializeField]
    private AudioClip punchSound;
    #endregion

    #region Internal members
    private RectTransform ingameCanvas;
    private Camera cam;
    public static event LevelExitDelegate levelExitEvent;
    #endregion

    private void Awake()
    {
        ingameCanvas = GameObject.FindGameObjectWithTag("IngameCanvas").GetComponent<RectTransform>();
        if (ingameCanvas == null)
            Debug.LogError("Ingame canvas not found!");

        if (crushedCircle == null)
            Debug.LogError("No circle image is set!");

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (cam == null)
            Debug.Log("Level end manager found no camera!");

        PlayerManager.AllPlayersDeadEventHandler += TweenEndScreenImage;
    }

    protected void TweenEndScreenImage()
    {
        DisableCanvasChildObjects();
        TweenCircleAndText();
        TweenCameraEffect();
        StartCoroutine(LoadNextScene());
    }

    private void TweenCircleAndText()
    {
        GameObject g = Instantiate(crushedCircle.gameObject);
        g.transform.SetParent(ingameCanvas.gameObject.transform, false);
        Image img = g.GetComponent<Image>();

        SoundManager.SoundManagerInstance.Play(wooshSound, Vector3.zero, 10f, 1f, false);
        img.rectTransform.localScale = Vector3.zero;
        LeanTween.scale(img.rectTransform, Vector3.one, tweenTime).setEase(LeanTweenType.easeOutElastic)
            .setOnComplete(TweenText);
    }

    private void TweenText()
    {
        GameObject g = Instantiate(crushedText.gameObject);
        g.transform.SetParent(ingameCanvas.gameObject.transform, false);
        Text txt = g.GetComponent<Text>();

        SoundManager.SoundManagerInstance.Play(punchSound, Vector3.zero, 15f, 1f, false);
        txt.rectTransform.localScale = Vector3.one * 8f;
        LeanTween.scale(txt.rectTransform, Vector3.one, crushedTextTweenTime).setEase(LeanTweenType.easeOutCubic);
    }

    protected void DisableCanvasChildObjects()
    {
        foreach (Transform child in ingameCanvas.transform)
            child.gameObject.SetActive(false);
    }

    protected void TweenCameraEffect()
    {
        GradientImageEffect effect = cam.GetComponent<GradientImageEffect>();
        effect.enabled = true;

        LeanTween.value(effect.gameObject, 0f, destinationIntensity, tweenTime).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) => {
                effect.intensity = val;
            });

        LeanTween.value(effect.gameObject, 0f, destinationGreenIntensity, tweenTime).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) => {
                effect.greenIntensity = val;
            });
    }

	protected IEnumerator LoadNextScene()
	{
		yield return new WaitForSeconds(tweenTime + waitTimeBeforeLevelLoad);
		OnLevelExit ();
		levelExitEvent = null;
		Application.LoadLevel (levelToLoad);
	}

	protected void OnLevelExit()
    {
		if (levelExitEvent != null)
			levelExitEvent();
	}
}