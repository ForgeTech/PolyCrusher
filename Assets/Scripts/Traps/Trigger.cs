using UnityEngine;
using System.Collections;
using System;

public class Trigger : MonoBehaviour {

    #region Class Members

    //the collider that calls the Trigger functions, keep for trap trigger call
    public Collider collided = null;

    #endregion

    #region Class Methods

    public void Awake()
    {
        GetComponentsInChildren<Renderer>()[0].material.color = Color.red;
    }

    //called when collider enters trigger
    protected void OnTriggerEnter(Collider other)
    {
        collided = other;
        GetComponentsInChildren<Animation>()[0].Play("onenter");
        GetComponentsInChildren<Renderer>()[0].material.color = Color.green;
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
        GetComponentsInChildren<Animation>()[0].Play("onexit");
        GetComponentsInChildren<Renderer>()[0].material.color = Color.red;
    }

    #endregion
}
