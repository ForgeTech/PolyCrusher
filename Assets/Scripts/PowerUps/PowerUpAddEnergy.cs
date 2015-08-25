using UnityEngine;
using System.Collections;

public class PowerUpAddEnergy : MonoBehaviour {

	public void Use(BasePlayer player) {
		player.Energy = player.MaxEnergy;
		Destroy(this);
	}
}
