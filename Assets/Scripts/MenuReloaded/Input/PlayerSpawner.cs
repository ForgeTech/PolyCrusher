using UnityEngine;
using System.Collections.Generic;
using InControl;
using System;

#if UNITY_EDITOR
//this class is pure nonsense, it will only be used for easy testing and stuff
public class PlayerSpawner : MonoBehaviour {


    private enum PlayerNames
    {
        Birdman, Charger, Fatman, Timeshifter, Babuschka, Pantomime, Tomic
    }

    private enum Controllers
    {
         Controller_1, Controller_2, Controller_3, Controller_4, Keyboard, None
    }


    [Header("Choose the needed characters")]

    [SerializeField]
    private PlayerNames[] chosenPlayerIndices;

    [Header("Choose which controller should be used for the players")]

    [SerializeField]
    private Controllers[] controllerNumber;

    [SerializeField]
    private GameObject editorSpawner;

    private PlayerSelectionContainer playerSelectionContainer;

    private List<InputDevice> gamePadDevices;
    private InputDevice keyboardDevice;


    private PlayerControlActions currentPlayerActions;

    private List<InputDevice> usedInputDevices;
    

	// Use this for initialization
	void Awake () {

        LevelEndManager.levelExitEvent += Reset;

        if (FindObjectOfType<PlayerSelectionContainer>() == null)
        {
            editorSpawner.SetActive(true);
            gamePadDevices = new List<InputDevice>();
            GetGamePadDevices();
            playerSelectionContainer = editorSpawner.GetComponent<PlayerSelectionContainer>();
            if (playerSelectionContainer != null)
            {
                usedInputDevices = new List<InputDevice>();

                if (chosenPlayerIndices.Length > 0 && chosenPlayerIndices.Length != controllerNumber.Length)
                {
                    Debug.Log("Much to learn you still have padawan!");
                }
                else
                {
                    for (int i = 0; i < chosenPlayerIndices.Length; i++)
                    {
                        if (controllerNumber[i] == Controllers.Keyboard)
                        {
                            playerSelectionContainer.playerInputDevices[i] = keyboardDevice;
                        }
                        else if(controllerNumber[i] == Controllers.None)
                        {
                            playerSelectionContainer.playerInputDevices[i] = InputDevice.Null;
                        }
                        else
                        {
                            if((int)controllerNumber[i] < gamePadDevices.Count)
                            {
                                playerSelectionContainer.playerInputDevices[i] = gamePadDevices[(int)controllerNumber[i]];
                            }
                            else
                            {
                                playerSelectionContainer.playerInputDevices[i] = gamePadDevices[gamePadDevices.Count-1];
                            }
                        }

                        playerSelectionContainer.playerActive[i] = true;
                        playerSelectionContainer.playerPrefabIndices[i] = (int)chosenPlayerIndices[i];
                    }
                    editorSpawner.tag = "GlobalScripts";
                }
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
	}

    private void GetGamePadDevices()
    {
        for(int i = 0; i < InputManager.Devices.Count; i++)
        {
            if(InputManager.Devices[i].Name!= "Smartphone Controller" )
            {
                if(InputManager.Devices[i].Name == "Keyboard Controller")
                {
                    keyboardDevice = InputManager.Devices[i];
                }
                else
                {
                    gamePadDevices.Add(InputManager.Devices[i]);
                }
            }
        }
    }


    private void Reset()
    {
        Destroy(editorSpawner);
    }

    

}
#endif
