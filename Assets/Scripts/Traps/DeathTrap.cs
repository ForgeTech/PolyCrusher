using UnityEngine;
using System.Collections;

public class DeathTrap : Trap,ITriggerable {

    #region Class Methods
    
    public override void Trigger(Collider other) {
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

    #endregion
}
