using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Menu script for the option menu
/// </summary>
public class OptionsMenu : MonoBehaviour
{

    public GameObject[] buttons;
    private GameObject[,] buttonArray;

    private GameObject[] optionArray;
    private int selectedOption;
    private int disabledOption;


    public GameObject[] resolutions;
    private int selectedResolution;

    public GameObject[] quality;
    private int selectedQuality;


    public GameObject[] fullscreen;
    private int selectedFullscreen;

    public GameObject[] vsync;
    private int selectedVsync;

    public GameObject[] antiAliasing;
    private int selectedAntiAliasing;

    private GameObject[] allTextObjects;
    private int counter;

    private int[] arrayLengths;
    public int maxItems;
    public int columns;
    public Vector2 selected;

    bool normalMovement;

    //public int selected;

    private bool moveLeft = false;
    private bool moveRight = false;
    private bool moveUp = false;
    private bool moveDown = false;

    public float inputCooldownTime;

    private int playerCount;
    public LevelStartInformation levelInfo;
    public PlayerNetCommunicate network;

    //The player slots of the four players. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlot = new bool[4];

    //The player slots of the four players for the phone. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlotPhone = new bool[4];

    public int currentPlayerCount;
    //bool newPhone = false;


    private bool selectedChange = false;
    private int maxPlayers = 4;


    private float inputCooldown;
    private float animationTime = 0.0f;
    private bool inputReceived = false;

    [Space(5)]
    [Header("Menu Sounds:")]
    [SerializeField]
    protected AudioClip acceptSound;
    [SerializeField]
    protected AudioClip switchSound;
    [SerializeField]
    protected AudioClip declineSound;


    //Delegates
    private event ButtonSwitchedEvent ButtonSwitchEvent;
    private event ButtonAcceptedEvent ButtonAcceptEvent;
    private event ButtonDeclinedEvent ButtonDeclineEvent;

    int oldCounter = 0;

    public float x;

    bool once;

    

    void Awake()
    {
        allTextObjects = new GameObject[50];
        counter = 0;


        AddTextObjects(buttons);
        AddTextObjects(resolutions);
        AddTextObjects(quality);
        AddTextObjects(vsync);
        AddTextObjects(fullscreen);
        AddTextObjects(antiAliasing);
        



        normalMovement = true;
        GameObject[] music = GameObject.FindGameObjectsWithTag("Music");

        if (music != null)
        {
            if (music.Length > 1)
            {
                Destroy(music[1]);
            }

            DontDestroyOnLoad(music[0]);
        }
        maxItems = 0;
        columns = 1;
        int count = 0;


        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                count++;
            }
            else
            {
                count = 0;
                columns++;
            }

            if (count > maxItems)
            {
                maxItems = count;
            }
        }


        buttonArray = new GameObject[columns, maxItems];
        arrayLengths = new int[columns];

        count = 0;
        int anotherCount = 0;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttonArray[count, anotherCount++] = buttons[i];
            }
            else
            {
                arrayLengths[count] = anotherCount;
                count++;
                anotherCount = 0;
            }

            if(i == buttons.Length - 1)
            {
                arrayLengths[count] = anotherCount;
            }


        }


        //buttonArray[0, 0].GetComponent<Button>().Select();
        selected = new Vector2(0, 0);

    }

    void Start()
    {
        //buttons[0].GetComponent<Button>().Select();

        // Register switch sound.
        ButtonSwitchEvent += PlaySwitchSound;
        ButtonAcceptEvent += PlayAcceptSound;
        ButtonDeclineEvent += PlayDeclineSound;

        inputCooldown = inputCooldownTime;

        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
        network = GameObject.FindObjectOfType<PlayerNetCommunicate>();

        playerSlotPhone = new bool[4];

        playerCount = Input.GetJoystickNames().Length;
        currentPlayerCount = playerCount;

        Debug.Log(playerCount);

        for (int i = 0; i < playerCount; i++)
        {

            playerSlot[i] = true;
            levelInfo.playerSlotTaken[i] = true;

        }

        for (int i = 0; i < levelInfo.phonePlayerSlotTaken.Length; i++)
        {
            if (levelInfo.phonePlayerSlotTaken[i] && playerCount < 4)
            {
                playerSlotPhone[i] = true;
                //levelInfo.phonePlayerSlotTaken[i] = true;
                currentPlayerCount++;

            }
        }

        GameObject.Find("GameName").GetComponent<Text>().text = network.gameName + " GAME";

        once = false;

        UpdateButtonTexts();


        //StartCoroutine("ScalePlayerImages");
        //StartCoroutine("TransformBetabanner");

    }


    private void ChangeLanguage(string language)
    {

        //ConfigScript.LanguageChange(language);
        LanguageFileReader.ChangeLanguage(language);
        Debug.Log(LanguageFileReader.selectedLanguage);

        UpdateButtonTexts();
    }



    IEnumerator ScalePlayerImages()
    {

        GameObject[] buttonsNew = new GameObject[3];

        buttonsNew[0] = GameObject.Find("Crush_Poly_Text");
        buttonsNew[1] = GameObject.Find("Game_Stats_Text");
        buttonsNew[2] = GameObject.Find("Leave_Game_Text");

        for (int i = 0; i < buttons.Length; i++)
        {
            //buttons[i].GetComponent<Image>().enabled = false;
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < buttonsNew.Length; i++)
        {
            Vector3 originalScale = buttonsNew[i].transform.localScale;
            buttonsNew[i].transform.localScale = Vector3.zero;
            buttonsNew[i].GetComponent<Image>().enabled = true;
            StartCoroutine(buttonsNew[i].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));
        }

    }



    /// <summary>
    /// Adds a new phone player to the game.
    /// </summary>
    public void HandlePhonePlayerJoin(int slot)
    {

        bool joined = false;

        if (currentPlayerCount < 4)
        {
            if (!joined && !playerSlotPhone[0])
            {

                playerSlotPhone[0] = true;
                levelInfo.phonePlayerSlotTaken[0] = true;
                joined = true;

            }

            if (!joined && !playerSlotPhone[1])
            {

                playerSlotPhone[1] = true;
                levelInfo.phonePlayerSlotTaken[1] = true;
                joined = true;

            }

            if (!joined && !playerSlotPhone[2])
            {

                playerSlotPhone[2] = true;
                levelInfo.phonePlayerSlotTaken[2] = true;
                joined = true;

            }

            if (!joined && !playerSlotPhone[3])
            {

                playerSlotPhone[3] = true;
                levelInfo.phonePlayerSlotTaken[3] = true;
                joined = true;

            }

            if (joined)
            {
                //newPhone = true;
            }

        }

    }

    void Update()
    {


        UpdatePlayerStatus();
        if (normalMovement)
        {
            buttonArray[(int)selected.x, (int)selected.y].GetComponent<Button>().Select();
        }else
        {
            optionArray[selectedOption].GetComponent<Button>().Select();

        }
        HideButtons();

        

        if (inputReceived)
        {
            inputCooldown -= Time.deltaTime;

            if (inputCooldown <= 0.0f)
            {
                inputCooldown = inputCooldownTime;
                inputReceived = false;
            }
        }

        if (normalMovement)
        {
            HandleInput();
            ProcessInput();

            HandleSelection();
        }else
        {
            HandleInput();
            ProcessOptionInput();
            HandleOptionSelection();
        }       

        ChangeImages();
    }


    private void HandleOptionSelection()
    {

        bool selection = false;
        for (int i = 0; i < maxPlayers; i++)
        {
            int runningNumber = i + 1;

            string submit = "P" + runningNumber + "_Ability";

            if (!selection && (Input.GetButtonDown(submit) || network.actionButton[i] == 1))// || Input.GetButtonDown("Selection")))
            {
                selection = true;
                if (network.actionButton[i] == 1)
                {
                    network.actionButton[i] = 0;
                }
               
                optionArray[selectedOption].GetComponent<Button>().onClick.Invoke();
            }


        }

    }



    private void ProcessOptionInput()
    {
        if (moveUp && selectedOption != 0)
        {
            selectedOption -= 1;
            OnButtonSwitched();
            moveUp = false;
        }


        if (moveDown && selectedOption != optionArray.Length - 1)
        {
            Debug.Log("Items " + optionArray.Length);
            selectedOption += 1;
            OnButtonSwitched();
            moveDown = false;
        }

        moveLeft = false;
        moveRight = false;
        moveUp = false;
        moveDown = false;
    }

  

    private void HideButtons()
    {

        for (int i = 0; i < buttonArray.GetLength(0); i++)
        {
            for (int j = 0; j < buttonArray.GetLength(1); j++)
            {

                if (buttonArray[i, j] != null)
                {
                    buttonArray[i, j].SetActive(true);

                }
            }
        }



        for (int i = 0; i < buttonArray.GetLength(0); i++)
        {
            for (int j = 0; j < buttonArray.GetLength(1); j++)
            {

                if (buttonArray[i, j] != null && j >= 1 && i != selected.x)
                {
                    buttonArray[i, j].SetActive(false);

                }
            }
        }
    }


    private void UpdatePlayerStatus()
    {


        int newControlCount = 0;
        currentPlayerCount = 0;

        for (int i = 0; i < playerSlot.Length; i++)
        {
            if (playerSlot[i])
            {
                newControlCount++;
                currentPlayerCount++;
            }

            if (playerSlotPhone[i])
            {
                currentPlayerCount++;
            }
        }

        bool controllerAdded = false;
        if (newControlCount < Input.GetJoystickNames().Length && currentPlayerCount < 4)
        {
            //playerSlot[Input.GetJoystickNames().Length - 1] = true;
            //levelInfo.playerSlotTaken[Input.GetJoystickNames().Length - 1] = true;
            for (int i = 0; i < playerSlot.Length; i++)
            {
                if (!controllerAdded && playerSlot[i] == false)
                {
                    playerSlot[i] = true;
                    levelInfo.playerSlotTaken[i] = true;
                    controllerAdded = true;
                }
            }
        }

    }

    private void HandleInput()
    {

        for (int i = 0; i < maxPlayers; i++)
        {

            float x = 0.0f;

            int runningNumber = i + 1;

            string curHorizontal = "P" + runningNumber + "_Horizontal";


            if (Input.GetAxis(curHorizontal) <= -0.2f || Input.GetAxis(curHorizontal) >= 0.2f)
            {
                x = Input.GetAxisRaw(curHorizontal);

            }
            else if (network.horizontal[i] <= -0.2f || network.horizontal[i] >= 0.2f)
            {

                x = network.horizontal[i];
            }




            float y = 0.0f;

            runningNumber = i + 1;

            string curVertical = "P" + runningNumber + "_Vertical";


            if (Input.GetAxis(curVertical) <= -0.2f || Input.GetAxis(curVertical) >= 0.2f)
            {
                y = Input.GetAxisRaw(curVertical);

            }
            else if (network.vertical[i] <= -0.2f || network.vertical[i] >= 0.2f)
            {

                y = network.vertical[i];
            }



            if (!inputReceived)
            {
                if (x < -0.5f && selected.x != 0)
                {
                    inputReceived = true;
                    moveLeft = true;
                }
                else if (x > 0.5f && selected.x != buttonArray.GetLength(0) - 1)
                {
                    inputReceived = true;
                    moveRight = true;
                }
                else if (y < -0.5f && selected.y != 0)
                {
                    inputReceived = true;
                    moveUp = true;
                }
                else if (y > 0.5f && selected.y != buttonArray.GetLength(1) - 1)
                {
                    inputReceived = true;
                    moveDown = true;
                }
            }
        }
    }

    private void ProcessInput()
    {
        if (moveLeft && selected.y == 0)
        {
            selected.x -= 1;

            OnButtonSwitched();
            moveLeft = false;
        }

        if (moveRight && selected.y == 0)
        {
            selected.x += 1;
            OnButtonSwitched();
            moveRight = false;
        }

        if (moveUp && selected.y != 0)
        {
            selected.y -= 1;
            OnButtonSwitched();
            moveUp = false;
        }


        if (moveDown && selected.y != arrayLengths[(int)selected.x]-1)
        {
            Debug.Log("Items "+arrayLengths[(int)selected.x]);
            selected.y += 1;
            OnButtonSwitched();
            moveDown = false;
        }

        moveLeft = false;
        moveRight = false;
        moveUp = false;
        moveDown = false;

    }



    private void HandleSelection()
    {
        bool selection = false;
        for (int i = 0; i < maxPlayers; i++)
        {
            int runningNumber = i + 1;

            string submit = "P" + runningNumber + "_Ability";

            if (!selection && (Input.GetButtonDown(submit) || network.actionButton[i] == 1))// || Input.GetButtonDown("Selection")))
            {
                selection = true;
                if (network.actionButton[i] == 1)
                {
                    network.actionButton[i] = 0;
                }
                buttonArray[(int)selected.x, (int)selected.y].GetComponent<Button>().onClick.Invoke();
            }
        }
    }

   

    /// <summary>
    /// Plays the switch sound.
    /// </summary>
    protected void PlaySwitchSound()
    {
        if (switchSound != null)
            SoundManager.SoundManagerInstance.Play(switchSound, Vector3.zero);
    }

    /// <summary>
    /// Plays the accept sound.
    /// </summary>
    protected void PlayAcceptSound()
    {
        if (acceptSound != null)
            SoundManager.SoundManagerInstance.Play(acceptSound, Vector3.zero);
    }

    /// <summary>
    /// Plays the decline sound.
    /// </summary>
    protected void PlayDeclineSound()
    {
        if (declineSound != null)
            SoundManager.SoundManagerInstance.Play(declineSound, Vector3.zero);
    }

    /// <summary>
    /// Event method for button switch.
    /// </summary>
    protected void OnButtonSwitched()
    {
        if (ButtonSwitchEvent != null)
            ButtonSwitchEvent();
    }

    /// <summary>
    /// Event method for button accept.
    /// </summary>
    protected void OnButtonAccepted()
    {
        if (ButtonAcceptEvent != null)
            ButtonAcceptEvent();
    }

    /// <summary>
    /// Event method for button decline.
    /// </summary>
    protected void OnButtonDeclined()
    {
        if (ButtonDeclineEvent != null)
            ButtonDeclineEvent();
    }

    void ChangeImages()
    {

        int slotCounter = 0;

        for (int i = 0; i < playerSlot.Length; i++)
        {
            if (playerSlot[i])
            {
                slotCounter++;
            }
        }

        for (int i = 0; i < playerSlotPhone.Length; i++)
        {
            if (playerSlotPhone[i])
            {
                slotCounter++;
            }
        }

        if (oldCounter != slotCounter)
        {
            if (slotCounter == 0)
            {
                GameObject.Find("Player1_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player2_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player3_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player4_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
            }
            else if (slotCounter == 1)
            {
                GameObject.Find("Player1_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player2_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player3_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player4_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
            }
            else if (slotCounter == 2)
            {
                GameObject.Find("Player1_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player2_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player3_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
                GameObject.Find("Player4_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
            }
            else if (slotCounter == 3)
            {
                GameObject.Find("Player1_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player2_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player3_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player4_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_not_joined_button 1");
            }
            else if (slotCounter == 4)
            {
                GameObject.Find("Player1_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player2_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player3_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
                GameObject.Find("Player4_Status").GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/player_joined_button 1");
            }
        }


        oldCounter = slotCounter;

    }


    //button click events


    public void LanguageEnglish()
    {
        ChangeLanguage("English");
    }

    public void LanguageGerman()
    {
        ChangeLanguage("German");
    }

    public void LanguageEspanol()
    {
        ChangeLanguage("Espanol");
    }


    public void FullscreenChange()
    {
        string curFullscreen = Screen.fullScreen.ToString();        
        PrepareOptions(fullscreen, curFullscreen);
    }


    public void AntialiasingChange()
    {
        string curAntialiasing = QualitySettings.antiAliasing.ToString();
        PrepareOptions(antiAliasing, curAntialiasing);
    }

    public void VsyncChange()
    {
        string curVsync = QualitySettings.vSyncCount.ToString();
        PrepareOptions(vsync, curVsync);
    }



    public void QualityChange()
    {
        string curQuality = QualitySettings.GetQualityLevel().ToString();
        PrepareOptions(quality, curQuality);
    }


    public void ResolutionChange()
    {
        string curRes = Screen.width.ToString();
        PrepareOptions(resolutions, curRes);
    }


    private void PrepareOptions(GameObject[] options, string toCompare)
    {
        normalMovement = false;
        optionArray = options;
        for(int i = 0; i <optionArray.Length; i++)
        {
            optionArray[i].SetActive(true);
            if (toCompare == optionArray[i].name)
            {
                optionArray[i].GetComponent<Button>().interactable = false;
                disabledOption = i;
            }
        }

        if (disabledOption != 0)
        {
            optionArray[0].GetComponent<Button>().Select();
            selectedOption = 0;
        }
        else
        {
            optionArray[1].GetComponent<Button>().Select();
            selectedOption = 1;
        }


    }

    public void Deactivatefullscreen()
    {
        Screen.fullScreen = false;
        Canvas.ForceUpdateCanvases();
        DeactivateOptionItems();

    }

    public void ActivateFullscreen()
    {
        Screen.fullScreen = true;
        Canvas.ForceUpdateCanvases();
        DeactivateOptionItems();
    }

   
    public void DeactivateAntialiasing()
    {
        QualitySettings.antiAliasing = 0;
        DeactivateOptionItems();
    }

    public void AA2x()
    {
        QualitySettings.antiAliasing = 2;
        DeactivateOptionItems();
    }

    public void AA4x()
    {
        QualitySettings.antiAliasing = 4;
        DeactivateOptionItems();
    }

    public void AA8x()
    {
        QualitySettings.antiAliasing = 8;
        DeactivateOptionItems();
    }



    public void ActivateVsync()

    {
        QualitySettings.vSyncCount = 1;
        DeactivateOptionItems();
    }

    public void DeactivateVsync()
    {
        QualitySettings.vSyncCount = 0;
        DeactivateOptionItems();
    }


    public void SetQualityLow()
    {
        QualitySettings.SetQualityLevel(0,true);
        DeactivateOptionItems();
    }

    public void SetQualityMed()
    {
        QualitySettings.SetQualityLevel(2, true);
        DeactivateOptionItems();
    }

    public void SetQualityHigh()
    {
        QualitySettings.SetQualityLevel(4, true);
        DeactivateOptionItems();
    }

    public void SetQualityMax()
    {
        QualitySettings.SetQualityLevel(5, true);
        DeactivateOptionItems();
    }


    public void SetResolutionFHD()
    {
        Screen.SetResolution(1920, 1080, true);
        DeactivateOptionItems();
    }

    public void SetResolutionHDplus()
    {
        Screen.SetResolution(1600, 900, true);
        DeactivateOptionItems();
    }

    public void SetResolutionHD13()
    {
        Screen.SetResolution(1366, 768, true);
        DeactivateOptionItems();
    }

    public void SetResolutionHD()
    {
        Screen.SetResolution(1280, 720, true);
        DeactivateOptionItems();
    }

    private void DeactivateOptionItems()
    {
        for(int i = 0; i < optionArray.Length; i++)
        {
            optionArray[i].SetActive(false);
            optionArray[i].GetComponent<Button>().interactable = true;
        }
        normalMovement = true;
    }

    public void BackToMain()
    {
        //if (!once)
        {
            if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().transitionFinished)
            {
                //once = true;

                OnButtonAccepted();
                GameObject.Find("_StartMenu").GetComponent<StartMenu>().ChangeScenes("OptionMenuObject(Clone)", "Scenes/Menu/MainMenuObject", true);
            }
        }

    }


    private void UpdateButtonTexts()
    {
        for (int i = 0; i < allTextObjects.Length; i++)
        {
            if (allTextObjects[i] != null)
            {
                if (LanguageFileReader.GetLanguageObject(allTextObjects[i].name) != null)
                {
                    if (allTextObjects[i].activeSelf == false)
                    {
                        allTextObjects[i].SetActive(true);
                        allTextObjects[i].GetComponentInChildren<Text>().text = LanguageFileReader.GetLanguageObject(allTextObjects[i].name);
                        Debug.Log("changed language");

                        allTextObjects[i].SetActive(false);
                    }
                    else
                    {
                        allTextObjects[i].GetComponentInChildren<Text>().text = LanguageFileReader.GetLanguageObject(allTextObjects[i].name);
                        Debug.Log("changed language");
                    }
                }
            }
        }






    }


    private void AddTextObjects(GameObject[] array)
    {
        for(int i =0; i <array.Length; i++)
        {
            if(array[i] != null)
            {
                allTextObjects[counter] = array[i];
                counter++;
            }
        }
    }
    


}