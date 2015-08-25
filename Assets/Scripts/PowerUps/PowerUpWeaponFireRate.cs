using UnityEngine;
using System.Collections;

public class PowerUpWeaponFireRate :  PowerUp {
	// Lasting time of the power up
	private float powerUpTime;

	// The weapon of the current player
	private Weapon weapon;

	// The player who picks up the power up
	private BasePlayer activePlayer;
	
	// The damagemultiplier of the powerup which is added to the weapon of the current player
	private int fireRateMultiplier;
	
	// The value of the usual damage
	private float usualFireRate;

	public void Use(BasePlayer player, float time, int powerUpValue, bool addPermanently) {
		if (addPermanently){
			float powerValue = powerUpValue / 100f;
			player.PlayerWeapon.WeaponFireRate *= powerValue;
			Destroy(this);
		} else {
			fireRateMultiplier = powerUpValue;
			powerUpTime = time;
			
			activePlayer = player;
			usualFireRate = player.PlayerWeapon.WeaponFireRate;
			player.PlayerWeapon.WeaponFireRate /= fireRateMultiplier;

			StartCoroutine("WaitUntilReset");
		}

	}

	protected IEnumerator WaitUntilReset()
	{
		yield return new WaitForSeconds(powerUpTime);
		activePlayer.PlayerWeapon.WeaponFireRate = usualFireRate;
		Destroy(this);
		Destroy(transform.parent);
	} 

	// Stopps coroutine and start it again
	public void breakAndRestart(){
		StopCoroutine("WaitUntilReset");

		StartCoroutine("WaitUntilReset");
	}


}
