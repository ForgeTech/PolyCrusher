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
    public string levelName;

    public GameObject[] playerPrefabs;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}