using UnityEngine;
using System.Collections;

public class PowerUpCut : PowerUp
{
    // Reference to the linesystem.
    protected LineSystem lineSystem;

    protected void Start()
    {
        lineSystem = GameObject.FindObjectOfType<LineSystem>();
    }

    public override void Use()
    {
        if (lineSystem != null)
            lineSystem.ActivateCutting();

        StartCoroutine("WaitUntilReset");
    }

    protected IEnumerator WaitUntilReset()
    {
        yield return new WaitForSeconds(powerUpActiveTime);

        Destroy(this);
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