using UnityEngine;
using System.Collections;
using InControl;

public class PauseMenuManager : MonoBehaviour {

    private PlayerControlActions playerActions;

    [SerializeField]
    private GameObject menuElements;

    [SerializeField]
    private GameObject staticElements;

    private Vector3 startPosition = new Vector3(2000, 0, 0);

    [SerializeField]
    private float tweenTime = 0.5f;

    private bool pauseScreenActivated = false;
    private bool animationFinished = true;

    //private Camera camera;
    private GradientImageEffect gradient;


    private AbstractMenuManager menuManager;


    [Header("Camera effect setting")]
    [SerializeField]
    private float destinationIntensity = 0.7f;

    [SerializeField]
    private float destinationGreenIntensity = 0.3f;


    void OnEnable()
    {
        playerActions = PlayerControlActions.CreateWithGamePadBindings();
        gameObject.transform.position = startPosition;

        gradient = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().GetComponent<GradientImageEffect>();

        menuManager = GetComponentInChildren<AbstractMenuManager>();
        menuManager.enabled = false;
        menuManager.SetMenuInputActive(false);
    }

    void OnDisable()
    {
        playerActions.Destroy();
    }

	// Update is called once per frame
	void Update () {
        if (playerActions.Pause)
        {
            if (!pauseScreenActivated)
            {
                 PauseGame();
            }
        }

        if (playerActions.Pause || playerActions.Back)
        {
            if (pauseScreenActivated)
            {
                ResumeGame();
            }

        }
	}

    private void SetMenuActive(bool setActive)
    {
        menuElements.SetActive(setActive);
       
    }

    private void TimeTween(bool animateIn)
    {
        float first = 1.0f;
        float second = 0.0f;

        if (!animateIn)
        {
            first = 0;
            second = 1.0f;
        }

        LeanTween.value(gameObject, first, second, tweenTime * 2f).setOnUpdate(
            (float val) => { Time.timeScale = val; }
        ).setEase(LeanTweenType.easeOutSine).setUseEstimatedTime(true).setOnComplete(()=> {

            animationFinished = true;
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                InputManager.Devices[i].StopVibration();
            }
        });
    }

    private void PauseTween(bool animateIn)
    {

        Vector3 first = startPosition;
        Vector3 second = Vector3.zero;

        if (!animateIn)
        {
            first = Vector3.zero;
            second = startPosition;
        }

        LeanTween.value(gameObject, first, second, tweenTime).setOnUpdate(
            (Vector3 pos) => { gameObject.transform.position = pos; }
        ).setEase(LeanTweenType.easeInQuad).setUseEstimatedTime(true).setOnComplete(()=>TweenCameraEffect(animateIn) );
    }

    private void TweenCameraEffect(bool animateIn)
    {
        gradient.enabled = true;

        menuManager.enabled = animateIn;
        SetMenuActive(animateIn);

        float first = 0.0f;
        float second = destinationIntensity;

        if (!animateIn)
        {
            first = destinationIntensity;
            second = 0.0f;
        }
        LeanTween.value(gradient.gameObject, first, second, tweenTime).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) => {
                gradient.intensity = val;
            }).setUseEstimatedTime(true);

        first = 0.0f;
        second = destinationGreenIntensity;

        if (!animateIn)
        {
            first = destinationGreenIntensity;
            second = 0.0f;
        }

        LeanTween.value(gradient.gameObject, first, second, tweenTime).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) => {
                gradient.greenIntensity = val;
            }).setUseEstimatedTime(true);
    }


    public void ResumeGame()
    {
        if (animationFinished)
        {
            PauseTween(false);
            TimeTween(false);
            pauseScreenActivated = false;
            animationFinished = false;
        }
    } 

    private void PauseGame()
    {
        if (animationFinished)
        {         
            PauseTween(true);
            TimeTween(true);
            pauseScreenActivated = true;
            animationFinished = false;
        }
    }



  
}
