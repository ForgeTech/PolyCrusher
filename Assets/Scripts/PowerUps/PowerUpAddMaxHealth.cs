using UnityEngine;
using System.Collections;

public class PowerUpAddMaxHealth :  PowerUp {

	public void Use(BasePlayer player, int addValue) {
		player.MaxHealth += addValue;
		Destroy(this);
	}
}
