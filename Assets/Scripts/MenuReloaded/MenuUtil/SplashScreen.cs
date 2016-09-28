using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Prime31.TransitionKit;

public class SplashScreen : MonoBehaviour {

    [SerializeField]
	private string nextLevelName;

    [SerializeField]
    private Shader transitionShader;

    [SerializeField]
    private RawImage image;

    private MovieTexture movie;

    void Start () {
        movie = (MovieTexture)image.texture;
        movie.Play();
        SoundManager.SoundManagerInstance.Play(movie.audioClip, Vector2.zero, AudioGroup.MenuSounds);

        StartCoroutine(ChangeScene(movie.duration));
    }

    IEnumerator ChangeScene(float waitTime)
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
        
        yield return new WaitForSeconds(waitTime);
        image.color = Color.black;
        TransitionKit.instance.transitionWithDelegate(fishEye);
    }
}
