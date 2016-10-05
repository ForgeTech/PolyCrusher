using UnityEngine;
using InControl;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    private PlayerControlActions playerActions;
    private List<PlayerControlActions> playerActionList;
    private PlayerSelectionContainer playerSelectionContainer;

    [SerializeField]
    private GameObject menuElements;

    [SerializeField]
    private GameObject staticElements;

    private Vector3 startPosition = new Vector3(0, 1100f, 0);

    [SerializeField]
    private float tweenTime = 0.5f;

    private bool pauseScreenActivated = false;
    private bool animationFinished = true;

    //private Camera camera;
    private GradientImageEffect gradient;

    private AbstractMenuManager menuManager;

    [Header("Camera effect setting")]
    [SerializeField]
    private float destinationIntensity = 1f;

    [SerializeField]
    private float destinationGreenIntensity = 0f;

    private BaseSteamManager steamManager;

    private bool gameEnded = false;

    private void Start()
    {
        playerActions = PlayerControlActions.CreateWithGamePadBindings();
        gameObject.transform.position = startPosition;

        gradient = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().GetComponent<GradientImageEffect>();

        menuManager = GetComponentInChildren<AbstractMenuManager>();
        menuManager.enabled = false;
        menuManager.SetMenuInputActive(false);

        LevelEndManager.levelExitEvent += DeRegister;

        playerActionList = new List<PlayerControlActions>();
        GameObject g = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (g != null)
        {
            playerSelectionContainer = g.GetComponent<PlayerSelectionContainer>();
            if (playerSelectionContainer != null)
            {
                for (int i = 0; i < playerSelectionContainer.playerInputDevices.Length; i++)
                {
                    if (playerSelectionContainer.playerInputDevices[i] != null)
                    {
                        Debug.Log("Player Action " + i + " is: " + playerSelectionContainer.playerInputDevices[i].Name);
                        PlayerControlActions p = PlayerControlActions.CreateWithGamePadBindings();
                        p.Device = playerSelectionContainer.playerInputDevices[i];
                        playerActionList.Add(p);
                    }
                }
            }
        }
       

        steamManager = SteamManager.Instance;
        if (steamManager != null)
        {
            steamManager.OnOverlayActivated += OnOverlayActivated;
        }

        PlayerManager.AllPlayersDeadEventHandler += () => {
            gameEnded = true;
        };
    }

    private void DeRegister()
    {
        playerActions.Destroy();

        for(int i = 0; i < playerActionList.Count; i++)
        {
            playerActionList[i].Destroy();        
        }

        if (steamManager!=null)
        {
            steamManager.OnOverlayActivated -= OnOverlayActivated;
        }
    }

	private void Update ()
    {
        if (playerSelectionContainer == null)
        {
            if (playerActions.Pause.WasPressed && !gameEnded)
            {
                if (!pauseScreenActivated)
                    PauseGame();
            }

            if (playerActions.Pause.WasPressed || playerActions.Back.WasPressed)
            {
                if (pauseScreenActivated)
                    ResumeGame();
            }
        }
        else
        {
            if(PauseWasPressed() && !gameEnded && !pauseScreenActivated)
            {
                PauseGame();
            }

            if((PauseWasPressed() || BackWasPressed()) && pauseScreenActivated)
            {
                ResumeGame();
            }
        }
	}

    private bool PauseWasPressed()
    {
        for(int i = 0; i < playerActionList.Count;  i++)
        {            
            if (playerActionList[i].Pause.WasPressed)
            {
                return true;
            }
        }
        return false;
    }

    private bool BackWasPressed()
    {
        for (int i = 0; i < playerActionList.Count; i++)
        {
            if (playerActionList[i].Back.WasPressed)
            {
                return true;
            }
        }
        return false;
    }

    private void SetMenuActive(bool setActive)
    {
        menuElements.SetActive(setActive);
        Debug.Log("- Pause Menu: " + setActive);
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

        menuManager.enabled = animateIn;
        menuManager.SetMenuInputActive(animateIn);
       // SetMenuActive(animateIn);

        LeanTween.value(gameObject, first, second, tweenTime).setOnUpdate(
            (Vector3 pos) => { gameObject.transform.position = pos; }
        ).setEase(LeanTweenType.easeInQuad).setUseEstimatedTime(true).setOnComplete(()=>TweenCameraEffect(animateIn) );
    }

    private void TweenCameraEffect(bool animateIn)
    {
        gradient.enabled = true;

       

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
            }).setUseEstimatedTime(true).setOnComplete(()=> { animationFinished = true; });
    }

    private void OnOverlayActivated()
    {
        if (!pauseScreenActivated)
        {
            PauseGame();
        }
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