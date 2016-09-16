using UnityEngine;
using System.Collections;

public class PowerUpCut : PowerUp
{
    // Reference to the linesystem.
    protected LineSystem lineSystem;
    public AudioClip laserSound;
    public float volume;

    private GameObject go;

    public override void Use()
    {
        lineSystem = GameObject.FindObjectOfType<LineSystem>();
        if (lineSystem != null)
            lineSystem.ActivateCutting(powerUpActiveTime);

        StartCoroutine("WaitUntilReset");
        go = new GameObject();
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = laserSound;
        source.loop = true;
        source.volume = volume;
        go = Instantiate(go);
    }

    protected IEnumerator WaitUntilReset()
    {
        yield return new WaitForSeconds(powerUpActiveTime);
        
        Destroy(this);
        Destroy(go);
        Destroy(transform.parent);
    }


    /// <summary>
    /// Stopps coroutine and start it again
    /// </summary>
    public void breakAndRestart()
    {
        StopCoroutine("WaitUntilReset");

        StartCoroutine("WaitUntilReset");
    }
}