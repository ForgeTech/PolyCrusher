using UnityEngine;
using System.Collections.Generic;
using InControl;

#if UNITY_EDITOR
//this class is pure nonsense, it will only be used for easy testing and stuff
public class PlayerSpawner : MonoBehaviour {


    private enum PlayerNames
    {
        Birdman, Charger, Fatman, Timeshifter, Babuschka, Pantomime
    }

    private enum Controllers
    {
        Keyboard, Controller_1, Controller_2, Controller_3, Controller_4
    }


    [Header("Choose the needed characters")]

    [SerializeField]
    private PlayerNames[] chosenPlayerIndices;

    [Header("Choose which controller should be used for the players")]

    [SerializeField]
    private Controllers[] controllerNumber;

    //[HideInInspector]

    [Header("Keep of the grass!")]
    [SerializeField]
    private GameObject[] playerPrefabs;


    private bool[] controllerAssigned;

    private bool isActive = false;

    private BasePlayer[] basePlayers;

    private int assignedControls;


    private Transform spawnPosition;

    //private PlayerControlActions playerAction;

    private int maxPlayers = 4;

    private InputDevice current;

    private PlayerControlActions currentPlayerActions;

    private List<InputDevice> usedInputDevices;
    

	// Use this for initialization
	void Awake () {

        if (FindObjectOfType<PlayerSelectionContainer>() == null)
        {
            isActive = true;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (isActive)
        {
            assignedControls = 0;
            controllerAssigned = new bool[chosenPlayerIndices.Length];
            usedInputDevices = new List<InputDevice>();

            if (spawnPosition == null && GameObject.FindGameObjectsWithTag("PlayerSpawn").Length > 0)
                spawnPosition = GameObject.FindGameObjectsWithTag("PlayerSpawn")[0].transform;

            if (chosenPlayerIndices.Length>0 && chosenPlayerIndices.Length!=controllerNumber.Length)
            {
                Debug.Log("Much to learn you still have padawan!");
                isActive = false;
            }
            else
            {
                //playerAction = PlayerControlActions.CreateWithGamePadBindings();
                
                GameObject prefab;

                int amount = chosenPlayerIndices.Length;
                if (chosenPlayerIndices.Length > maxPlayers)
                {
                    amount = maxPlayers;
                }


                RumbleManager rumbleManager = RumbleManager.Instance;
                basePlayers = new BasePlayer[amount];
                for (int i = 0; i < amount; i++)
                {
                    prefab = Instantiate(playerPrefabs[(int)chosenPlayerIndices[i]]) as GameObject;
                    prefab.GetComponent<NavMeshAgent>().enabled = false;
                    prefab.transform.position = spawnPosition.position;
                    basePlayers[i] = prefab.GetComponent<BasePlayer>();
                    basePlayers[i].RumbleManager = rumbleManager;
                    prefab.GetComponent<NavMeshAgent>().enabled = true;
                 
                }
            }
        }
	}
	

	// Update is called once per frame
	void Update () {
        if (isActive)
        {
            if (!AllPlayersAssigned())
            {
                if (assignedControls == 0 && KeyboardNeeded() )
                {
                    if (KeyboardNeeded())
                    {
                        foreach(InputDevice inputDevice in InputManager.Devices)
                        {                        
                            if (inputDevice != null && inputDevice.Name == "Keyboard Controller")
                            {
                                current = inputDevice;
                            }
                        }
                        if (AssignDevices(current))
                        {
                            usedInputDevices.Add(current);
                        }
                        assignedControls++;
                    }
                }else
                {
                    current = InputManager.ActiveDevice;

                    if (current.Name!="None" && DeviceAvailable())
                    {
                        if (AssignDevices(current))
                        {
                            usedInputDevices.Add(current);
                        }
                        assignedControls++;
                    }             
                }
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
	}


    private bool AllPlayersAssigned()
    {
        int count = 0;

        for(int i = 0; i < controllerAssigned.Length; i++) {
            if (controllerAssigned[i])
            {
                count++;
            }
        }

        return (count == controllerAssigned.Length) ? true : false;
    }

    private bool KeyboardNeeded()
    {
        foreach(Controllers c in controllerNumber)
        {
            if(c == Controllers.Keyboard)
            {
                return true;
            }
        }

        return false;
    }

    private bool AssignDevices(InputDevice inputDevice)
    {
        bool assigned = false;

        for(int i = 0; i < chosenPlayerIndices.Length; i++)
        {
            if(controllerNumber[i] == (Controllers)assignedControls)
            {
                basePlayers[i].InputDevice = inputDevice;
                basePlayers[i].PlayerActions = PlayerControlActions.CreateWithGamePadBindings();
                basePlayers[i].PlayerActions.Device = inputDevice;

                controllerAssigned[i] = true;
                assigned = true;
            }
        }

        return assigned;
    }

    private bool DeviceAvailable()
    {
        foreach(InputDevice i in usedInputDevices)
        {
            if(i == current)
            {
                return false;
            }
        }
        return true;
    }


}
#endif
