using UnityEngine;
using System.Collections;

public class DeathTrap : MonoBehaviour,ITriggerable {

    #region Class Members
    //how long the trap needs to reset
    [SerializeField]
    public float trapActiveTime = 0.5f;

    //trap can only be triggered if this is false
    protected bool isActive = false;

    #endregion

    #region Class Methods

    //trigger method that manages kill & trap animation
    public void Trigger(Collider other) {
        if (isActive == false)
        {
            isActive = true;
            StartCoroutine(WaitForActive());
            GetComponent<Animation>().Play("strike");

            if (other.tag == "Player")
            {
                // get the BasePlayer of the Game Object
                BasePlayer player = other.GetComponent<BasePlayer>();
                player.InstantKill();
            }

            if (other.tag == "Enemy")
            {
                // get the BaseEnemy of the Game Object
                BaseEnemy enemy = other.GetComponent<BaseEnemy>();
                enemy.InstantKill();
            }
        }
    }

    //calls the trigger method if player/enemy collides with the trap
    void OnTriggerEnter(Collider other)
    {
        Trigger(other);
    }

    //keeps the trap from triggering too often
    IEnumerator WaitForActive()
    {
        yield return new WaitForSeconds(trapActiveTime);
        isActive = false;
    }

    #endregion
}
