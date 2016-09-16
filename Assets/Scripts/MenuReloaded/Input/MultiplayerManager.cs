using UnityEngine;
using System.Collections.Generic;
using InControl;
using System.Collections;
using Prime31.TransitionKit;

public enum PlayerSlot
{
    None = -1,
    Player1 = 0,
    Player2 = 1,
    Player3 = 2,
    Player4 = 3
}

public class MultiplayerManager : MonoBehaviour
{

    #region Inspector
    [SerializeField]
    private AudioClip finishedSound;
    #endregion

    #region variables

    //----------public


    public delegate void FinalSelectionHandler(float tweentime);
    public event FinalSelectionHandler FinalSelectionExecuted;
    public event FinalSelectionHandler FinalSelectionStoped;

    //----------private


    //maximum amount of players
    private static int MAX_PLAYER_COUNT = 4;

    //final screen tween time
    private static float TWEEN_TIME = 0.3f;

    //WaitforSeconds
    private WaitForSeconds waitTime;

    //current player count
    private int playerCount = 0;

    //current amount of players that finished selecting a character
    private int playerReadyCount = 0;

    //character menus can select
    private bool singleControls = true;

    //global selection using all devices in usedInputDevices
    private bool globalControls = false;

    //inputListener
    private PlayerControlActions gamepadListener;
    private PlayerControlActions specificGamepadListener;

    //InputDevices that are assigned to a specific slot already are stored here
    private List<InputDevice> usedInputDevices = new List<InputDevice>(MAX_PLAYER_COUNT);
    
    //The SlotContainer objects are stored here
    private List<SlotContainer> slots = new List<SlotContainer>(MAX_PLAYER_COUNT);
    
    //needed references
    private PlayerSelectionContainer playerSelectionContainer;
    private CharacterSelectionHelper characterSelectionHelper;

    private string previousLevelName = "MenuReloadedLevelSelection";

    #endregion

    #region SlotContainer class

    //holds information of the slots charactermenumanager and its inputdevice
    private class SlotContainer
    {
        public SlotContainer(CharacterMenuManager menuManager, InputDevice inputDevice)
        {
            this.menuManager = menuManager;
            this.inputDevice = inputDevice;
        }

        private CharacterMenuManager menuManager;
        private InputDevice inputDevice;

        public InputDevice InputDevice
        {
            get { return inputDevice; }
            set
            {
                inputDevice = value;
                PlayerControlActions playerActions = PlayerControlActions.CreateWithGamePadBindings();
                playerActions.Device = value;
                menuManager.SetPlayerControlActions(playerActions);
            }
        }

        public bool IsFree()
        {
            return inputDevice == null ? true : false;
        }

        public PlayerSlot GetPlayerSlot()
        {
            return menuManager.PlayerSlot;
        }

        public void SetMenuInputActive(bool status)
        {
            menuManager.SetMenuInputActive(status);
        }

        public void Deselect()
        {
            menuManager.Deselect();
        }
    }

    #endregion

    #region methods

    void OnEnable()
    {
        playerCount = 0;
        playerReadyCount = 0;
        waitTime = new WaitForSeconds(TWEEN_TIME);

        slots.Clear();
        usedInputDevices.Clear();


        GatherNeededReferences();

        InputManager.OnDeviceDetached += OnDeviceDetached;
        gamepadListener = PlayerControlActions.CreateWithGamePadBindings();
        specificGamepadListener = PlayerControlActions.CreateWithGamePadBindings();
      

        characterSelectionHelper.OnCharacterSelected += IncreasePlayerReadyCount;
        characterSelectionHelper.OnCharacterDeselected += DecreasePlayerReadyCount;
    }


    private void GatherNeededReferences()
    {
        //getting all the character menu manager scripts
        GameObject[] go = GameObject.FindGameObjectsWithTag("CharacterMenuManager");
        CharacterMenuManager characterMenuManager;
        if (go != null)
        {
            for (int i = 0; i < go.Length; i++)
            {
                characterMenuManager = go[i].GetComponent<CharacterMenuManager>();
                characterMenuManager.SetMenuInputActive(false);
                slots.Add(new SlotContainer(characterMenuManager, null));
            }
            SortSlots();
        }
        else
        {
            Debug.LogError("No CharacterMenuManager found!");
        }


        //getting the player selection container
        GameObject g1 = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (g1 != null)
        {
            playerSelectionContainer = g1.GetComponent<PlayerSelectionContainer>();
            if (playerSelectionContainer == null)
            {
                Debug.LogError("No PlayerSelectionContainer found!");
            }
        }
        else
        {
            Debug.LogError("No Global Scripts GameObject found!");
        }

        //getting the character selection helper
        GameObject g2 = GameObject.FindGameObjectWithTag("CharacterMenuHelper");
        if (g2 != null)
        {
            characterSelectionHelper = g2.GetComponent<CharacterSelectionHelper>();
            if (characterSelectionHelper == null)
            {
                Debug.LogError("Chracter Selection Helper is null!");
            }
        }
        else
        {
            Debug.LogError("No Character Selection GameObject found!");
        }
    }

    private void SortSlots()
    {
        SlotContainer temp;
        for(int i = 0; i < slots.Count; i++)
        {
            for(int j = 1; j < slots.Count; j++)
            {
                if(slots[j-1].GetPlayerSlot() > slots[j].GetPlayerSlot())
                {
                    temp = slots[j];
                    slots[j] = slots[j - 1];
                    slots[j - 1] = temp;
                }
            }
        }
    }

    //void OnDisable()
    private void OnDestroy()
    {
        InputManager.OnDeviceDetached -= OnDeviceDetached;
        characterSelectionHelper.OnCharacterSelected -= IncreasePlayerReadyCount;
        characterSelectionHelper.OnCharacterDeselected -= DecreasePlayerReadyCount;
        gamepadListener.Destroy();
        specificGamepadListener.Destroy();
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

    void Update()
    {
        if (singleControls && playerCount < MAX_PLAYER_COUNT)
        {
            if (JoinButtonWasPressedOnListener(gamepadListener))
            {
                InputDevice inputDevice = InputManager.ActiveDevice;
                gamepadListener.ExcludeDevices.Add(inputDevice);
                if (ThereIsNoPlayerUsingThisGamePad(inputDevice))
                {
                    AssignInputDevice(inputDevice);
                }
            }

            if (BackButtonWasPressedOnListener(gamepadListener)){

                Application.LoadLevel(previousLevelName);
            }
        }

        if (globalControls)
        {
            if (specificGamepadListener.Join.WasPressed)
            {
                SaveSelectionInformationToContainer();
                DontDestroyOnLoad(SoundManager.SoundManagerInstance.Play(finishedSound, Vector3.zero, AudioGroup.MenuSounds));
                ChangeScene();
            }

            if (specificGamepadListener.Back.WasPressed)
            {               
                SlotContainer slot = FindSlotContainer(InputManager.ActiveDevice);
                if (slot != null)
                {
                    slot.Deselect();
                }          
            }
        }
    }

    private SlotContainer FindSlotContainer(InputDevice activeDevice)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if(slots[i].InputDevice == activeDevice)
            {
                return slots[i];
            }
        }
        return null;
    }

    private void ChangeScene()
    {
        Application.LoadLevel(Application.loadedLevel+1);       
    }

    private void SaveSelectionInformationToContainer()
    {
        int index = 0;
        for(int i = 0; i < characterSelectionHelper.SelectionMap.Count; i++)
        {
            if (characterSelectionHelper.SelectionMap[i].selected)
            {
                index = (int)characterSelectionHelper.SelectionMap[i].selectedBySlot;
                playerSelectionContainer.playerActive[index] = true;
                playerSelectionContainer.playerPrefabIndices[index] = i;
                playerSelectionContainer.playerInputDevices[index] = slots[index].InputDevice;
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
            specificGamepadListener.IncludeDevices.Add(inputDevice);
            playerCount++;
        }
    }
    
    private int GetFirstFreeSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsFree())
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

    private bool BackButtonWasPressedOnListener(PlayerControlActions actions)
    {
        return actions.Back;
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


    private void IncreasePlayerReadyCount(int index, PlayerSlot player)
    {
        playerReadyCount++;
        CheckAllPlayersReady();
    }

    private void DecreasePlayerReadyCount(int index, PlayerSlot player)
    {
        CheckFinalSelectionStop();
        playerReadyCount--;
    }


    private void CheckAllPlayersReady()
    {
        if (playerCount == playerReadyCount)
        {
            singleControls = false;            
            if (FinalSelectionExecuted != null)
            {
                FinalSelectionExecuted(TWEEN_TIME);
            }
            SetMenuInputActive(false);
            StartCoroutine(ActivateGlobalControls());
        }
    }

    private void CheckFinalSelectionStop()
    {
        if (playerReadyCount == playerCount)
        {
            globalControls = false;           
            if (FinalSelectionStoped != null)
            {
                FinalSelectionStoped(TWEEN_TIME);
            }
            StartCoroutine(ActivateSingleControls());
        }
    }

    private IEnumerator ActivateSingleControls()
    {
        yield return waitTime;
        singleControls = true;
        SetMenuInputActive(true);
    }


    private IEnumerator ActivateGlobalControls()
    {
        yield return waitTime;
        globalControls = true;
    }

    private void SetMenuInputActive(bool status)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            slots[i].SetMenuInputActive(status);
        }
    }

    #endregion
}
