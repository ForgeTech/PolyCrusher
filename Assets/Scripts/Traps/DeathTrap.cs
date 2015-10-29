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
                //player.gameObject.AddComponent<PolyExplosion>();
            }

            if (other.tag == "Enemy")
            {
                // get the BaseEnemy of the Game Object
                BaseEnemy enemy = other.GetComponent<BaseEnemy>();
                enemy.InstantKill();
                //enemy.gameObject.AddComponent<PolyExplosion>();
            }
        }
        StartCoroutine(WaitForReset());
    }

    protected IEnumerator WaitForReset()
    {
        yield return new WaitForSeconds(trapActiveTime);
        triggers[0].resetTrigger();
    }

    #endregion
}
