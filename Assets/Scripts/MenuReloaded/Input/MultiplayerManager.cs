using UnityEngine;
using System.Collections.Generic;
using InControl;
using System;

public class MultiplayerManager : MonoBehaviour {

    //----------public

    [Header("Player Selection Information Container")]
    public GameObject playerSelectionContainer;

    [Header("Character Selection Menu Prefab")]
    public GameObject charSelectionPrefab;

    [Header("Placement coordinates")]
    public List<Vector3> placementCoordinates;

    //----------private

    private const int maxPlayers = 4;

    private PlayerSelectionContainer playerSelectionInstance;
    private List<Player> players = new List<Player>(maxPlayers);
  
    private PlayerControlActions keyboardListener;
    private PlayerControlActions gamepadListener;



    private int index = 0;

    void OnEnable()
    {        
        InputManager.OnDeviceDetached += OnDeviceDetached;
        //keyboardListener = PlayerControlActions.CreateWithKeyboardBindings();
        gamepadListener = PlayerControlActions.CreateWithGamePadBindings();

      

        playerSelectionInstance = GameObject.FindObjectOfType<PlayerSelectionContainer>();
        if (playerSelectionInstance==null)
        {
            playerSelectionInstance = ((GameObject) Instantiate(playerSelectionContainer, Vector3.zero, Quaternion.identity) ).GetComponent<PlayerSelectionContainer>();           
        }        
    }

    void OnDisable()
    {
        InputManager.OnDeviceDetached -= OnDeviceDetached;
       
        gamepadListener.Destroy();
        //keyboardListener.Destroy();

    
    }

    void OnDeviceDetached(InputDevice inputDevice)
    {
        Player player = FindPlayersUsingGamePad(inputDevice);
        if (player != null)
        {
            RemovePlayer(player);
        }
    }

    void RemovePlayer(Player player)
    {
        placementCoordinates.Insert(0, player.transform.position);
        players.Remove(player);
        player.Actions = null;
        Destroy(player.gameObject);
    }

    void Start()
    {
        InputManager.OnDeviceDetached += OnDeviceDetached;    
            
    }

    void Update()
    {
        if (players.Count < maxPlayers)
        {
            if (JoinButtonWasPressedOnListener(gamepadListener))
            {
                var inputDevice = InputManager.ActiveDevice;

                if (ThereIsNoPlayerUsingGamePad(inputDevice))
                {
                    CreatePlayer(inputDevice);
                }
            }

            //if (JoinButtonWasPressedOnListener(keyboardListener))
            //{
            //    if (ThereIsNoPlayerUsingKeyboard())
            //    {
            //        CreatePlayer(null);
            //    }
            //}
        }       
    }



    Player CreatePlayer(InputDevice inputDevice)
    {
        if (players.Count < maxPlayers)
        {
            // Pop a position off the list. We'll add it back if the player is removed.
            Vector3 playerPosition = placementCoordinates[0];
            placementCoordinates.RemoveAt(0);

            GameObject gameObject = (GameObject)Instantiate(charSelectionPrefab, playerPosition, Quaternion.identity);
            Player player = gameObject.GetComponent<Player>();

            if (inputDevice == null)
            {                
                player.Actions = keyboardListener;
                playerSelectionInstance.playerPrefabIndices[index] = 0;   
            }
            else
            {
                // Create a new instance and specifically set it to listen to the
                // given input device (joystick).
                PlayerControlActions actions = PlayerControlActions.CreateWithGamePadBindings();
                actions.Device = inputDevice;
                player.Actions = actions;


               
                playerSelectionInstance.playerInputDevices[index] = inputDevice;                
                playerSelectionInstance.playerPrefabIndices[index] = 0;
            }

            

            playerSelectionInstance.playerActive[index] = true;

            index++;

            players.Add(player);

            return player;
        }

        return null;
    }


   
    bool JoinButtonWasPressedOnListener(PlayerControlActions actions)
    {
        return actions.Join;
    }

    bool JoinButtonWasPressedOnDevice(InputDevice inputDevice)
    {
        return inputDevice.Action1;
    }



    Player FindPlayersUsingGamePad(InputDevice inputDevice)
    {
        int playerCount = players.Count;
        for (int i = 0; i < playerCount; i++)
        {
            Player player = players[i];
            if (player.Actions.Device == inputDevice)
            {
                return player;
            }
        }

        return null;
    }


    bool ThereIsNoPlayerUsingGamePad(InputDevice inputDevice)
    {
        return FindPlayersUsingGamePad(inputDevice) == null;
    }


    Player FindPlayerUsingKeyboard()
    {
        int playerCount = players.Count;
        for (int i = 0; i < playerCount; i++)
        {
            Player player = players[i];
            if (player.Actions == keyboardListener)
            {
                return player;
            }
        }

        return null;
    }


    bool ThereIsNoPlayerUsingKeyboard()
    {
        return FindPlayerUsingKeyboard() == null;
    }




    void OnGUI()
    {
        const float h = 22.0f;
        var y = 10.0f;

        GUI.Label(new Rect(10, y, 300, y + h), "Active players: " + players.Count + "/" + maxPlayers);
        y += h;

        if (players.Count < maxPlayers)
        {
            GUI.Label(new Rect(10, y, 300, y + h), "Press a button to join!");
            y += h;
        }
    }


  
}
