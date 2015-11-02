using UnityEngine;
using System.Collections;
using System;

public class Trap : MonoBehaviour,ITriggerable {

    #region Class Members

    //how long the trap is active
    [SerializeField]
    protected float trapActiveTime = 0.5f;

    //the triggers that are connected to the trap
    [SerializeField]
    protected Trigger[] triggers;

    //the player meshes that are used for the poly explosion
    [SerializeField]
    protected GameObject[] playerMeshes;

    //trap can only be triggered if this is false
    protected bool isActive = true;

    #endregion


    #region Class Methods

    //trigger method that manages kill & trap animation, will be overwritten for every individual kind of trap
    public virtual void Trigger(Collider other){}

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

    //sets trap active on awake
    public void Awake()
    {
        ResetTrap();
    }

    //calls the trigger method if all triggers are active with reference on the collider that entered the very FIRST trigger
    public void Update()
    {

        int counter = 0;
        for(int i = 0; i<triggers.Length; i++)
        {
            if (triggers[i].collided != null)
            {
                counter++;
            }
        }

        if(counter == triggers.Length)
        {
            Trigger(triggers[0].collided);
        }
    }

    #endregion
}
