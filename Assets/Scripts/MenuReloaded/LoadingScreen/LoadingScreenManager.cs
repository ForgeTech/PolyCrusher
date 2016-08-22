using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Prime31.TransitionKit;

public class LoadingScreenManager : MonoBehaviour {

    [Header("Hints are to be inserted here")]
    [SerializeField]
    private Hint[] hints;

    [Header("Needed references")]
    [SerializeField]
    private Image hintImage;

    [SerializeField]
    private Text hintTitle;

    [SerializeField]
    private Text hintText;

    [Header("Shader for transition")]
    [SerializeField]
    private Shader transitionShader;

    private string levelName;
    private bool levelLoaded = false;
    private WaitForSeconds waitTime = new WaitForSeconds(2.5f);
    private WaitForSeconds fadeTime = new WaitForSeconds(0.8f);
    private PlayerSelectionContainer playerSelectionContainer;
    private AudioSource audioSource;

    private bool loadingFinished = false;

    void Start () {
        GetLevelName();
        SetHint();
        Fade();       
      
	}

    private void GetLevelName()
    {
        GameObject g = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (g != null)
        {
            playerSelectionContainer = g.GetComponent<PlayerSelectionContainer>();
            if (playerSelectionContainer != null)
            {
                levelName = playerSelectionContainer.levelName;
            }
            else
            {
                Debug.LogError("No Player Selection Container Component found!");
            }
        }
        else
        {
            Debug.LogError("No Global Scripts Gameobject found!");
        }
    }

    private void SetHint()
    {
        int index = Random.Range(0, hints.Length);

        hintImage.sprite = hints[index].hintImage;
        hintTitle.text = hints[index].hintTitle;
        hintText.text = hints[index].hintText;
    }


  
    private void ChangeScene()
    { 
        FishEyeTransition fishEye = new FishEyeTransition()
        {
            nextScene = levelName,
            duration = 1.0f,
            size = 0.0f,
            zoom = 10.0f,
            colorSeparation = 5.0f,
            fishEyeShader = this.transitionShader
        };
        TransitionKit.instance.transitionWithDelegate(fishEye);
    }

    private void Fade()
    {

        audioSource = playerSelectionContainer.GetComponent<AudioSource>();       
        StartCoroutine(FadeAudio(audioSource));
        StartCoroutine(ChangeSceneAfterWaitTime());

    }

    private IEnumerator ChangeSceneAfterWaitTime()
    {
        yield return waitTime;
        audioSource.volume = 0;
        ChangeScene();

    }


    private IEnumerator FadeAudio(AudioSource audioSource)
    {
        while (audioSource.volume > 0.0f)
        {
            audioSource.volume -= Time.deltaTime / 0.8f;
            yield return null;
        }       
    }

	
}
