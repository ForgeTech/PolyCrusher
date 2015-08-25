using UnityEngine;
using System.Collections;

public class PowerUpWeaponDamage : PowerUp {
	// Lasting time of the power up
	private float powerUpTime;

	// The weapon of the current player
	private Weapon weapon;

	// The player who picks up the power up
	private BasePlayer activePlayer;

	// The damagemultiplier of the powerup which is added to the weapon of the current player
	private int damageMultiplier;

	// The value of the usual damage
	private int usualDamage;

	public void Use(BasePlayer player, float time, int powerUpValue, bool addPermanently) {
		if (addPermanently) {
			player.PlayerWeapon.WeaponDamage += powerUpValue;
			Destroy(this);
		} else {
			damageMultiplier = powerUpValue;
			powerUpTime = time;
			
			activePlayer = player;
			usualDamage = player.PlayerWeapon.WeaponDamage;
			player.PlayerWeapon.WeaponDamage *= damageMultiplier;
			
			StartCoroutine("WaitUntilDamageReset");
		}

	}
	
	protected IEnumerator WaitUntilDamageReset()
	{
		yield return new WaitForSeconds(powerUpTime);
		activePlayer.PlayerWeapon.WeaponDamage = usualDamage;
		Destroy(this);
	} 

	public void breakAndRestart(){
		StopCoroutine("WaitUntilDamageReset");
		
		StartCoroutine("WaitUntilDamageReset");
	}
}
