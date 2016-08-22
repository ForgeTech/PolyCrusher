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
    private GameObject endScreenImage;

    [Header("Tweening setting")]
    [SerializeField]
    private float tweenTime = 0.5f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [Header("Camera effect setting")]
    [SerializeField]
    private float destinationIntensity = 0.7f;

    [SerializeField]
    private float destinationGreenIntensity = 0.3f;
    #endregion

    #region Internal members
    private RectTransform ingameCanvas;
    private Camera cam;
	public static event LevelExitDelegate levelExitEvent;
    #endregion

    private void Awake ()
    {
        ingameCanvas = GameObject.FindGameObjectWithTag("IngameCanvas").GetComponent<RectTransform>();
        if (ingameCanvas == null)
            Debug.LogError("Ingame canvas not found!");

        if (endScreenImage == null)
            Debug.LogError("No end screen image is set!");

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (cam == null)
            Debug.Log("Level end manager found no camera!");

		PlayerManager.AllPlayersDeadEventHandler += TweenEndScreenImage;
	}

    protected void TweenEndScreenImage()
    {
        DisableCanvasChildObjects();

        GameObject endScreen = Instantiate(endScreenImage);
        endScreen.transform.SetParent(ingameCanvas.gameObject.transform, false);
        Debug.Log("<b>Instantiated end image!</b>");

        Image img = endScreen.GetComponent<Image>();
        img.rectTransform.anchoredPosition.Set(ingameCanvas.rect.width, 0f);

        LeanTween.value(img.gameObject, ingameCanvas.rect.width, 0, tweenTime).setEase(easeType)
            .setOnUpdate((float val) => {
                img.rectTransform.anchoredPosition = new Vector2(val, img.rectTransform.anchoredPosition.y);
            });

        TweenCameraEffect();

        StartCoroutine(LoadNextScene());
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