using UnityEngine;
using UnityEngine.UI;

public class MoviePlay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    protected bool playLooped = true;

    private MovieTexture movie;
	
	private void Start ()
    {
        movie = (MovieTexture)GetComponent<RawImage>().texture;
        movie.loop = playLooped;
        movie.Play();
	}
}