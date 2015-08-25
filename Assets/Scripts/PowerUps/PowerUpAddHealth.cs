using UnityEngine;
using System.Collections;

public class PowerUpAddHealth :  PowerUp {

	public void Use(BasePlayer player) {
		player.Health = player.MaxHealth;
		Destroy(this);
	}
}
