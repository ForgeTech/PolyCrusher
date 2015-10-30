using UnityEngine;
using System.Collections;

public class DeathTrap : Trap,ITriggerable {

    // Enum of the Death Traps to show it as a dropdown menu
    public enum DeathTrapEnum
    {
        bearTrap, floorTrap, hlTrap
    }

    [SerializeField]
    private DeathTrapEnum type;

    #region Class Methods

    public override void Trigger(Collider other) {
        if (isActive == false)
        {
            isActive = true;
            StartCoroutine(WaitForActive());

            if(type == DeathTrapEnum.bearTrap)
            {
                GetComponent<Animation>().Play("strike");
            } else if (type == DeathTrapEnum.floorTrap)
            {
                GetComponent<Animation>().Play("spike");
            }

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
