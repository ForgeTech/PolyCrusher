using UnityEngine;
using System.Collections;

/// <summary>
/// Contains data about the needed level information for each start.
/// Information -> Selected Level, Selected player (Which Controller and Class)
/// </summary>
public class LevelStartInformation : MonoBehaviour 
{    
    //Index of the level
    public int levelIndex;

    //Contains the types of the players, if no player was selected -> null
    public string[] playerSlot;
    public string[] phonePlayerSlot;

    public bool[] playerSlotTaken;
    public bool[] phonePlayerSlotTaken;

    void Awake()
    {
        //Don't destroy this object the obtain the level information.
        DontDestroyOnLoad(this);


        playerSlot = new string[4];
        phonePlayerSlot = new string[4];

        playerSlotTaken = new bool[4];
        phonePlayerSlotTaken = new bool[4];

        //Init arrays with null
        for (int i = 0; i < playerSlot.Length; i++)
        {
            playerSlot[i] = null;
            phonePlayerSlot[i] = null;
            playerSlotTaken[i] = false;
            phonePlayerSlotTaken[i] = false;
        }
    }


    /// <summary>
    /// Clears the player arrays.
    /// </summary>
    public void ClearPlayerArrays()
    {
        for (int i = 0; i < playerSlot.Length; i++)
        {
            playerSlot[i] = null;
            phonePlayerSlot[i] = null;
        }
    }

	void Start () 
    {
        
	}
}