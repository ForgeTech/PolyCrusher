using UnityEngine;
using System.Collections;
using System;

public class Trigger : MonoBehaviour {

    #region Class Members

    [SerializeField]
    public Material onEnter;

    [SerializeField]
    public Material onExit;

    [SerializeField]
    public float radius = 0.5f;

    //the collider that calls the Trigger functions, keep for trap trigger call
    public Collider collided = null;

    #endregion

    #region Class Methods

    //sets material color on awake
    public void Awake()
    {
        GetComponentsInChildren<Renderer>()[0].material = onExit;
    }

    public void Update()
    {
        Collider[] c = Physics.OverlapSphere(gameObject.transform.position, radius, (1 << 8) | (1 << 9));
        //Debug.Log(c.Length);
        if(c.Length>0)
        {
            if (!collided)
            {
                collided = c[0];
                if (GetComponentsInChildren<Animation>()[0])
                {
                    GetComponentsInChildren<Animation>()[0].Play("onenter");
                }
                GetComponentsInChildren<Renderer>()[0].material = onEnter;
            }
        } else
        {
            if (collided)
            {
                collided = null;
                if (GetComponentsInChildren<Animation>()[0])
                {
                    GetComponentsInChildren<Animation>()[0].Play("onexit");
                }
                GetComponentsInChildren<Renderer>()[0].material = onExit;
            }
        }

    }

    #endregion
}
