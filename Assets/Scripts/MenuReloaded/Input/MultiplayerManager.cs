using UnityEngine;
using System.Collections.Generic;
using InControl;
using System;

public class MultiplayerManager : MonoBehaviour {


    //----------public

    [Header("Player Selection Container Prefab goes here")]
    public GameObject playerSelectionContainer;


    public delegate void FinalSelectionHandler();
    public event FinalSelectionHandler FinalSelectionExecuted;
    public event FinalSelectionHandler FinalSelectionStoped;

    //----------private

    private const int maxPlayers = 4;

    private PlayerSelectionContainer playerSelectionInstance;
    private AbstractMenuManager[] menuManagers;
    private List<InputDevice> usedInputDevices;
  
    private PlayerControlActions gamepadListener;
    private CharacterSelectionHelper characterSelectionHelper;

    private List<SlotContainer> slots;

    private int index = 0;

    private int playerCount;
    private int playerReadyCount;

    private bool finalSelection;


    private class SlotContainer
    {
        public SlotContainer(AbstractMenuManager menuManager, InputDevice inputDevice)
        {
            this.menuManager = menuManager;
            this.inputDevice = inputDevice;
        }

        private AbstractMenuManager menuManager;
        private InputDevice inputDevice;    

        public InputDevice InputDevice
        {
            get { return inputDevice; }
            set {
                inputDevice = value;
                PlayerControlActions playerActions = PlayerControlActions.CreateWithGamePadBindings();
                playerActions.Device = value;
                menuManager.SetPlayerControlActions(playerActions);
                menuManager.SetMenuInputActive(true);
            }
        }

        public bool isFree()
        {
            return inputDevice == null ? true : false;
        }
    }



    void OnEnable()
    {
        slots = new List<SlotContainer>(maxPlayers);
        AbstractMenuManager[] menuManagers = FindObjectsOfType<AbstractMenuManager>();
        foreach(AbstractMenuManager menuManager in menuManagers)
        {
            menuManager.SetMenuInputActive(false);
            slots.Add(new SlotContainer(menuManager, null));
        }

        InputManager.OnDeviceDetached += OnDeviceDetached;
        gamepadListener = PlayerControlActions.CreateWithGamePadBindings();
        usedInputDevices = new List<InputDevice>();
        playerCount = 0;
        playerReadyCount = 0;
        finalSelection = false;

        playerSelectionInstance = GameObject.FindObjectOfType<PlayerSelectionContainer>();
        if (playerSelectionInstance==null)
        {
            playerSelectionInstance = ((GameObject) Instantiate(playerSelectionContainer, Vector3.zero, Quaternion.identity) ).GetComponent<PlayerSelectionContainer>();           
        }

        characterSelectionHelper = GameObject.FindObjectOfType<CharacterSelectionHelper>();
        if (characterSelectionHelper == null)
        {
            Debug.LogError("Chracter Selection Helper is null!");
        }

        characterSelectionHelper.OnCharacterSelected += IncreasePlayerReadyCount;
        characterSelectionHelper.OnCharacterDeselected += DecreasePlayerReadyCount;


        menuManagers = FindObjectsOfType<AbstractMenuManager>();
    }

    void OnDisable()
    {
        InputManager.OnDeviceDetached -= OnDeviceDetached;       
        gamepadListener.Destroy();        
    }

    void OnDeviceDetached(InputDevice inputDevice)
    {
        SlotContainer slot = FindPlayersUsingGamePad(inputDevice);
        if (slot != null)
        {
            RemoveAssignedControls(slot);
        }
    }

    void RemoveAssignedControls(SlotContainer slot)
    {
        slot.InputDevice = null;
    }

    void Start()
    {
        InputManager.OnDeviceDetached += OnDeviceDetached;    
    }

    void Update()
    {
        if (playerCount < maxPlayers)
        {
            if (JoinButtonWasPressedOnListener(gamepadListener))
            {
                InputDevice inputDevice = InputManager.ActiveDevice;

                if (ThereIsNoPlayerUsingThisGamePad(inputDevice))
                {
                    AssignInputDevice(inputDevice);
                }
            }
        }       
    }



    private void AssignInputDevice(InputDevice inputDevice)
    {
        int freeslot = GetFirstFreeSlot();

        if (freeslot != -1)
        {
            slots[freeslot].InputDevice = inputDevice;
            usedInputDevices.Add(inputDevice);
            playerCount++;
        }
    }



    private int GetFirstFreeSlot()
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isFree())
            {
                return i;
            }
        }
        return -1;
    }
   
    private bool JoinButtonWasPressedOnListener(PlayerControlActions actions)
    {
        return actions.Join;
    }

    private SlotContainer FindPlayersUsingGamePad(InputDevice inputDevice)
    {
        for (int i = 0; i < playerCount; i++)
        {
            SlotContainer slot = slots[i];
            if (slot.InputDevice == inputDevice)
            {
                return slot;
            }
        }
        return null;
    }


    private bool ThereIsNoPlayerUsingThisGamePad(InputDevice inputDevice)
    {
        return FindPlayersUsingGamePad(inputDevice) == null;
    }
  
    void OnGUI()
    {
        const float h = 22.0f;
        var y = 10.0f;

        GUI.Label(new Rect(10, y, 300, y + h), "Active players: " + playerCount + "/" + maxPlayers);
        y += h;

        if (playerCount < maxPlayers)
        {
            GUI.Label(new Rect(10, y, 300, y + h), "Press a button to join!");
            y += h;
        }
    }

    private void IncreasePlayerReadyCount(int index)
    {
        playerReadyCount++;
        CheckAllPlayersReady();
    }

    private void DecreasePlayerReadyCount(int index)
    {
        CheckFinalSelectionStop();
        playerReadyCount--;        
    }


    private void CheckAllPlayersReady()
    {
        if(playerCount == playerReadyCount && !finalSelection)
        {
            finalSelection = true;
            FinalSelectionExecuted();
        }
    }

    private void CheckFinalSelectionStop()
    {
        if(playerReadyCount == playerCount && finalSelection)
        {
            finalSelection = false;
            FinalSelectionStoped();
        }
    }
}
