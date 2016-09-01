using UnityEngine;
using InControl;

/// <summary>
/// A container which holds all relevant data for a game start.
/// This object is passed through the different menus and the specific data
/// is also filled in them.
/// </summary>
public class PlayerSelectionContainer : MonoBehaviour
{
    public readonly InputDevice[] playerInputDevices = new InputDevice[4];
    public readonly bool[] playerActive = new bool[4];
    public readonly int[] playerPrefabIndices = new int[4];

    [Header("Level information")]
    public string levelName;

    [Header("Beware of the order of the prefabs! Should be the same like in character menu.")]
    [Tooltip("The order of the character prefabs should be the same order like in the character menu."
        + " If wrong characters are spawned, check this order first!")]
    public GameObject[] playerPrefabs;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ResetPlayers()
    {
        for (int i = 0; i < playerInputDevices.Length; i++)
        {
            playerActive[i] = false;
            playerInputDevices[i] = null;
        }
    }
}