using UnityEngine;
using System.Collections;
using System;

public class Trap : MonoBehaviour,ITriggerable {

    #region Class Members
    //how long the trap needs to reset
    [SerializeField]
    protected float trapActiveTime = 0.5f;

    //trap can only be triggered if this is false
    protected bool isActive = false;

    #endregion

    #region Class Methods

    //trigger method that manages kill & trap animation
    public virtual void Trigger(Collider other){}

    //calls the trigger method if player/enemy collides with the trap
    void OnTriggerEnter(Collider other)
    {
        Trigger(other);
    }

    //keeps the trap from triggering too often
    protected virtual IEnumerator WaitForActive()
    {
        yield return new WaitForSeconds(trapActiveTime);
        ResetTrap();
    }

    //resets trap
    protected virtual void ResetTrap()
    {
        isActive = false;
    }

    #endregion
}
