using UnityEngine;

public class PlayerSelectionClearer : MonoBehaviour
{
	private void Start ()
    {
        PlayerSelectionContainer playerInfos = GameObject.FindObjectOfType<PlayerSelectionContainer>();
        playerInfos.ResetPlayers();
	}
}
