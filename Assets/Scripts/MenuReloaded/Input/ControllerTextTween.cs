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

    public GameObject shit;

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
    private Vector2 currentPosition = new Vector2(0,0);
    #endregion

    #region methods
    private void Awake()
    {
        timeTillDisappearance = new WaitForSeconds(displayTime);
        connectedText.gameObject.SetActive(false);
        disconnectedText.gameObject.SetActive(false);
        quitText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (now)
        {
            now = false;
            InitiateTweenIn(ControllerStateChange.Disconnected);
        }
    }

    public void InitiateTweenIn(ControllerStateChange controllerStateChange)
    {
        if (!tweenActive)
        {
            tweenActive = true;
            if (controllerStateChange == ControllerStateChange.Disconnected)
            {
                //TweenIn(disconnectedText);
            }
            else if( controllerStateChange == ControllerStateChange.Quit)
            {
                //TweenIn(quitText);
            }
            else
            {
                TweenIn(shit);
                shit.SetActive(true);
            }
        }
    }

    private void TweenIn(GameObject toShow)
    {
        toShow.SetActive(true);
        //currentPosition.x = toTween.position.x;
        //currentPosition.y = endHeight;
        //toTween.position = currentPosition;
        //Vector2 newPosition = new Vector2(toTween.position.x, startHeight);
        //LeanTween.value(toTween.gameObject, startHeight, endHeight, tweenTime)
        //   .setOnUpdate((float amount) =>
        //   {
        //       currentPosition.y = amount;
        //       toTween.position = newPosition;
        //   })
        //   .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => { StartCoroutine(TimeTillDisappearance(toTween)); });
    }

    private IEnumerator TimeTillDisappearance(RectTransform toTween)
    {
        Debug.Log("wait");
        yield return timeTillDisappearance;
        TweenOut(toTween);
    }

    private void TweenOut(RectTransform toTween)
    {
        currentPosition.x = toTween.position.x;
        LeanTween.value(toTween.gameObject, endHeight, startHeight, tweenTime)
          .setOnUpdate((float amount) =>
          {
              currentPosition.y = amount;
              toTween.position = currentPosition;
          })
          .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => { tweenActive = false; });
    }
    #endregion

}
