using UnityEngine;
using System.Collections;
using System;

public class Trigger : MonoBehaviour {

    #region Class Members

    [SerializeField]
    public Material onEnter;

    [SerializeField]
    public Material onExit;

    //the collider that calls the Trigger functions, keep for trap trigger call
    public Collider collided = null;

    #endregion

    #region Class Methods

    //sets material color on awake
    public void Awake()
    {
        GetComponentsInChildren<Renderer>()[0].material = onExit;
    }

    //called when collider enters trigger
    protected void OnTriggerEnter(Collider other)
    {
        collided = other;
        if (GetComponentsInChildren<Animation>()[0])
        {
            GetComponentsInChildren<Animation>()[0].Play("onenter");
        }

        GetComponentsInChildren<Renderer>()[0].material = onEnter;
    }
    
    //called when collider exits trigger
    protected void OnTriggerExit(Collider other)
    {
        resetTrigger();
    }

    //resets trigger
    public void resetTrigger()
    {
        collided = null;
        if (GetComponentsInChildren<Animation>()[0])
        {
            GetComponentsInChildren<Animation>()[0].Play("onexit");
        }
        GetComponentsInChildren<Renderer>()[0].material = onExit;
    }

    #endregion
}
