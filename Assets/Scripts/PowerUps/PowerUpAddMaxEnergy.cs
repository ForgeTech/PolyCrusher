using UnityEngine;
using System.Collections;

public class PowerUpAddMaxEnergy : MonoBehaviour {

	public void Use(BasePlayer player, int AddValue) {
		player.MaxEnergy += AddValue;
		Destroy(this);
	}
}
