using UnityEngine;
using System;
using UnityEngine.UI;

public class MoviePlay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    protected bool playLooped = true;

    [SerializeField]
    protected bool playOnStart = false;

    private MovieTexture movie;
	
	private void Start ()
    {
        try
        {
            movie = (MovieTexture)GetComponent<RawImage>().texture;
            movie.loop = playLooped;

            if (playOnStart)
                movie.Play();
            else
            {
                movie.Play();
                LeanTween.delayedCall(0.05f, () =>
                {
                    movie.Stop();
                });
            }
        }
        catch (Exception e)
        {
            Debug.Log("[MoviePlay]: Error in casting the MovieTexture!\n" + e.ToString());
        }
	}

    public void PauseMovie()
    {
        if (movie != null)
            movie.Pause();
    }

    public void ContinueMovie()
    {
        if (movie != null)
            movie.Play();
    }
}