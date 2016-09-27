using UnityEngine;
using System.Collections;
using Prime31.TransitionKit;

public class SplashScreen : MonoBehaviour {

    [SerializeField]
	private string nextLevelName;

    [SerializeField]
    private float displayTime;

    [SerializeField]
    private Shader transitionShader;

    void Start () {
        StartCoroutine(ChangeScene());
	}

    IEnumerator ChangeScene()
    {
        FishEyeTransition fishEye = new FishEyeTransition()
        {
            nextScene = nextLevelName,
            duration = 0.2f,
            size = 0.0f,
            zoom = 10.0f,
            colorSeparation = 5.0f,
            fishEyeShader = transitionShader
        };

        yield return new WaitForSeconds(displayTime);

        TransitionKit.instance.transitionWithDelegate(fishEye);
    }
}
