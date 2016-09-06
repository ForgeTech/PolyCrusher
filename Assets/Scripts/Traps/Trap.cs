using UnityEngine;
using System.Collections;
using System;

public delegate void TrapAction(Trap trap);

public class Trap : MonoBehaviour,ITriggerable {

    #region Class Members

    //how long the trap is active
    [SerializeField]
    public float trapActiveTime = 0.5f;

    //the triggers that are connected to the trap
    [SerializeField]
    protected Trigger[] triggers;

    //the player meshes that are used for the poly explosion
    [SerializeField]
    public GameObject[] playerMeshes;
    
    //specifies the trap gives boss damage
    public int bossDamage = 100;

    [SerializeField]
    public GameObject bossCuttingParticles;

    //the trap load bars, visualize when trap is active
    [SerializeField]
    public UnityEngine.UI.Image[] loadBars = null;

    //trap can only be triggered if this is false
    protected bool isActive = false;

    //sound event
    public static event TrapAction TrapTriggered;

    #endregion


    #region Class Methods

    //trigger method that manages kill & trap animation, will be overwritten for every individual kind of trap
    public virtual void Trigger(Collider other){}

    //keeps the trap from triggering too often
    protected virtual IEnumerator WaitForActive()
    {
        if (loadBars!=null)
        {
            float timer = 0.0f;
            while (timer <= trapActiveTime)
            {
                for(int i = 0; i<loadBars.Length; i++)
                {
                    loadBars[i].fillAmount = timer / trapActiveTime;
                }
                timer += Time.deltaTime;
                yield return 0;
            }
            ResetTrap();
        }
        else
        {
            yield return new WaitForSeconds(trapActiveTime);
            ResetTrap();
        }
    }

    //resets trap
    protected virtual void ResetTrap()
    {
        isActive = false;
    }

    //sets trap active false on awake
    public void Awake()
    {
        ResetTrap();
        LevelEndManager.levelExitEvent += ResetEvents;
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
            if (isActive == false)
            {
                isActive = true;
                StartCoroutine(WaitForActive());
                Trigger(triggers[0].collided);
            }
        }
    }

    //method for trigger event
    public void OnTrapTriggered()
    {
        if (TrapTriggered != null)
        {
            TrapTriggered(this);
        }
    }

    //reset event values 
    public void ResetEvents()
    {
        TrapTriggered = null;
    }

    #endregion
}
