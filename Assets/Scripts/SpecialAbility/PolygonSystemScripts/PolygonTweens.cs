using UnityEngine;
using UnityEngine.UI;

public delegate void PolygonStartAnimationHandler();
public delegate void PolygonEndAnimationHandler();

public class PolygonTweens : MonoBehaviour {

    #region variables
    [SerializeField]
    private float beginHeight = 10.0f;

    [SerializeField]
    private float finalHeight = 0.0f;

    [SerializeField]
    private float startAnimationTime = 0.3f;

    public static event PolygonStartAnimationHandler PolygonStartAnimationFinished;
    public static event PolygonEndAnimationHandler PolygonEndAnimationFinished;

    #region screenFlashVariables
    private int startAnimationCount = 0;
    private int executedAnimationCount = 0;

    private float screenFlashInTime = 0.2f;
    private float screenFlashOutTime = 1.0f;
    private float screenFlashAmount = 0.7f;
    private Color screenFlashColor = new Color(1,1,1,0);
    private Image screenFlashImage;
    #endregion

    #endregion

    #region methods

    #region awake
    void Awake()
    {
        GameObject screenFlashObject = new GameObject("Canvas Container");
        screenFlashObject.transform.SetParent(this.transform, false);

        Canvas canvas = screenFlashObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        screenFlashImage = screenFlashObject.AddComponent<Image>();
        screenFlashImage.color = Color.clear;

        LevelEndManager.levelExitEvent += Reset;
    }
    #endregion

    #region polygonStartAnimation
    public void InitiatePolyStartAnimation(float[] polygonPartHeightOffsets)
    {
        if (startAnimationCount == 0)
        {
            startAnimationCount = polygonPartHeightOffsets.Length - 1;
            PolyStartAnimation(polygonPartHeightOffsets);
        }
    }

    private void PolyStartAnimation(float[] polygonPartHeightOffsets)
    {
        LeanTween.value(gameObject, beginHeight, finalHeight, startAnimationTime)
            .setOnUpdate((float height) => { polygonPartHeightOffsets[startAnimationCount] = height; })
            .setEase(LeanTweenType.easeOutCirc).setOnComplete(() =>
            {              
                if (--startAnimationCount >= 0)
                {
                    PolyStartAnimation(polygonPartHeightOffsets);
                }
                else
                {
                    startAnimationCount = 0;
                    OnPolygonStartAnimationFinished();
                }         
            });      
    }
    #endregion

    #region polygonExecutedAnimation

    public void InitiatePolygonExecutedAnimation(float[] polygonPartHeightOffsets)
    {
        if (executedAnimationCount == 0)
        {
            executedAnimationCount = polygonPartHeightOffsets.Length - 1;
            PolygonExecutedAnimation(polygonPartHeightOffsets);
        }
    }

    private void PolygonExecutedAnimation(float[] polygonPartHeightOffsets)
    {
        LeanTween.value(gameObject, finalHeight, beginHeight, startAnimationTime)
            .setOnUpdate((float height) => { polygonPartHeightOffsets[executedAnimationCount] = height; })
            .setEase(LeanTweenType.linear).setOnComplete(() =>
            {
                if (--executedAnimationCount >= 0)
                {
                    PolygonExecutedAnimation(polygonPartHeightOffsets);
                }
                else
                {
                    executedAnimationCount = 0;
                    OnPolygonEndAnimationFinished();
                }              
            });
    }
    #endregion

    #region polygonMaterialTween
    public void InitiateMaterialTween()
    {

    }


    private void MaterialTween()
    {


    }


    #endregion

    #region updatePolygonDistanceColor
    public void UpdatePolygonDistanceColor()
    {


    }

    #endregion

    #region screenFlash
    public void InitiateScreenFlash()
    {
        ScreenFlashFadeIn();
    }

    private void ScreenFlashFadeIn()
    {
        LeanTween.value(gameObject, 0.0f, screenFlashAmount, screenFlashInTime)
            .setOnUpdate((float amount) => {
                screenFlashColor.a = amount;
                screenFlashImage.color = screenFlashColor; })
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                ScreenFlashFadeOut();
            });
    }

    private void ScreenFlashFadeOut()
    {
        LeanTween.value(gameObject, screenFlashAmount, 0.0f, screenFlashOutTime)
            .setOnUpdate((float amount) =>
            {
                screenFlashColor.a = amount;
                screenFlashImage.color = screenFlashColor;
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }

    #endregion

    #region eventMethods
    private void OnPolygonStartAnimationFinished()
    {
        if (PolygonStartAnimationFinished != null)
        {
            PolygonStartAnimationFinished();
        }
    }

    private void OnPolygonEndAnimationFinished()
    {
        if (PolygonEndAnimationFinished != null)
        {
            PolygonEndAnimationFinished();
        }
    }

    #endregion

    #region reset
    private void Reset()
    {
        PolygonStartAnimationFinished = null;
        PolygonEndAnimationFinished = null;
    }
    #endregion

    #endregion
}
