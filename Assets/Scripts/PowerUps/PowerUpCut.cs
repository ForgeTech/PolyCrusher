using UnityEngine;
using System.Collections;

public class PowerUpCut : PowerUp
{
    public override void Use()
    {
        //============================
        //TODO: Activate Cut script
        //============================

        StartCoroutine("WaitUntilReset");
    }

    protected IEnumerator WaitUntilReset()
    {
        yield return new WaitForSeconds(powerUpActiveTime);
        
        //============================
        //TODO: Deactivate Cut script
        //============================

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