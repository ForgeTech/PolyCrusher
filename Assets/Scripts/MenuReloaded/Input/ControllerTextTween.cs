using UnityEngine;
using System.Collections;



public class ControllerTextTween : MonoBehaviour {

    public bool now = false;

    #region variables
    [SerializeField]
    private RectTransform disconnectedText;

    [SerializeField]
    private RectTransform quitText;

    [SerializeField]
    private RectTransform connectedText;

    [SerializeField]
    private float startHeight = -800.0f;

    [SerializeField]
    private float endHeight = -480.0f;

    [SerializeField]
    private float tweenTime = 0.5f;

    [SerializeField]
    private float displayTime = 4.0f;

    private WaitForSeconds timeTillDisappearance;
    private bool tweenActive = false;
    private Vector2 currentPosition = new Vector2();
    #endregion

    #region methods
    private void Start()
    {
        timeTillDisappearance = new WaitForSeconds(displayTime);       
    }

    private void Update()
    {
        if (now)
        {
            now = false;
            InitiateTweenIn(ControllerStateChange.Connected);
        }
    }

    public void InitiateTweenIn(ControllerStateChange controllerStateChange)
    {
        if (!tweenActive)
        {
            tweenActive = true;
            if (controllerStateChange == ControllerStateChange.Disconnected)
            {
                TweenIn(disconnectedText);
            }
            else if( controllerStateChange == ControllerStateChange.Quit)
            {
                TweenIn(quitText);
            }
            else
            {
                TweenIn(connectedText);
            }
        }
    }

    private void TweenIn(RectTransform toTween)
    {
        LeanTween.value(gameObject, startHeight, endHeight, tweenTime)
           .setOnUpdate((float amount) =>
           {
               currentPosition.x = toTween.position.x;
              currentPosition.y = amount;
              toTween.position = currentPosition;
           })
           .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => { StartCoroutine(TimeTillDisappearance(toTween)); });
    }

    private IEnumerator TimeTillDisappearance(RectTransform toTween)
    {
        yield return timeTillDisappearance;
        TweenOut(toTween);
    }

    private void TweenOut(RectTransform toTween)
    {
        LeanTween.value(gameObject, endHeight, startHeight, tweenTime)
          .setOnUpdate((float amount) =>
          {
              currentPosition.x = toTween.position.x;
              currentPosition.y = amount;
              toTween.position = currentPosition;
          })
          .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => { tweenActive = false; });
    }
    #endregion

}
