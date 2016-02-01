using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NewCharacterSelectionScript : MonoBehaviour {


    public GameObject[] playerImages;
    public float inputCoolDownTime;
    private Vector3 scale;

    private float heightDistance;


    private Color[] imageColors;

    private PlayerNetCommunicate network;
    private LevelStartInformation levelInfo;
    private Canvas canvas;

    private GameObject[,] selectionImages; 
    public int[] selectedCharacters;
    private int[] hoveredCharacters;
    [SerializeField]
    private int[] playerCursorSlot;
    private bool[] playerSelected;


    private int[] transformBack;
    [SerializeField]
    private int[] currentControllerInput;
  
    private bool[] inputReceived;
    private float[] inputCooldown;
    private bool[] selectedChange;

    private int playerCount;
    private int currentPlayerCount;
    [SerializeField]
    private bool[] playerSlot = new bool[4];
    [SerializeField]
    private bool[] playerSlotPhone = new bool[4];
    private bool newPhone;

    private bool once = false;
    private Vector2[] middlePos;
    private string[] playerClassNames = new string[6] { "Birdman", "Charger", "Fatman", "Timeshifter", "Babuschka", "Pantomime" };


    private GameObject[] slides;
    private Text skipText;

    private GameObject[,] readyTexts;


    [Space(5)]
    [Header("Menu Sounds:")]
    [SerializeField]
    protected AudioClip acceptSound;
    [SerializeField]
    protected AudioClip switchSound;
    [SerializeField]
    protected AudioClip declineSound;
    [SerializeField]
    protected AudioClip beepSound;
    [SerializeField]
    protected AudioClip zeroSound;

    [Space(10)]
    [Header("Spawn sounds")]
    [SerializeField]
    protected MultipleAudioclips spawnBirdman;
    [SerializeField]
    protected MultipleAudioclips spawnTimeshifter;
    [SerializeField]
    protected MultipleAudioclips spawnCharger;
    [SerializeField]
    protected MultipleAudioclips spawnFatman;
    [SerializeField]
    protected MultipleAudioclips spawnBabuschka;
    [SerializeField]
    protected MultipleAudioclips spawnPantomime;



    //Delegates
    private event ButtonSwitchedEvent ButtonSwitchEvent;
    private event ButtonAcceptedEvent ButtonAcceptEvent;
    private event ButtonDeclinedEvent ButtonDeclineEvent;


    //async loading
    private AsyncOperation async;
    private bool levelLoaded = false;

    //countdown bools
    //private bool zero = false;
    //private bool one = false;
    //private bool two = false;
    //private bool three = false;


    //bool onceLevel = false;
    int playerHasChosen = 0;

    GameObject infoBar;
    GameObject gamerCountdown;


    Vector3 originalVecInfo;

    float gamerCountdownTime;
    float timeBeginning;
    bool first;

    Vector3 originalScaleGameStartsText;

    private bool onceLevel;

    // Use this for initialization
    void Start () {
       
        playerCursorSlot = new int[] {-1,-1,-1,-1 };
        middlePos = new Vector2[4];
        for(int i = 0; i < middlePos.Length; i++)
        {
            middlePos[i].Set(Screen.width / 5 + (Screen.width / 5 * i), Screen.height*1.5f );
        }

        selectedCharacters = new int[] {-1, -1, -1, -1 };
        hoveredCharacters = new int[] { 0, 0, 0, 0 };
        currentControllerInput = new int[4];
        playerSelected = new bool[4];
        inputReceived = new bool[4];
        inputCooldown = new float[] {inputCoolDownTime, inputCoolDownTime, inputCoolDownTime, inputCoolDownTime };
        selectedChange = new bool[4];

        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
        levelInfo.ClearPlayerArrays();
        network = GameObject.FindObjectOfType<PlayerNetCommunicate>();
        canvas = GameObject.FindObjectOfType<Canvas>();

        playerSlotPhone = new bool[4];

        playerCount = Input.GetJoystickNames().Length;
        currentPlayerCount = playerCount;

        scale = new Vector3();
        scale.Set(0.0f, 0.0f,1.0f);

        
        if (levelInfo != null)
        {
            currentPlayerCount = playerCount;

            for (int i = 0; i < playerCount; i++)
            {
                playerCursorSlot[i] = i;
                playerSlot[i] = true;
            }

            for (int i = 0; i < levelInfo.phonePlayerSlotTaken.Length; i++)
            {
                if (levelInfo.phonePlayerSlotTaken[i])
                {
                    playerCursorSlot[playerCount + i] = 4 + i;
                    playerSlotPhone[i] = true;
                    currentPlayerCount++;

                }
            }
        }


        
        heightDistance = Screen.height / 3;

        selectionImages = new GameObject[4,playerImages.Length];
        readyTexts = new GameObject[4, playerImages.Length];
        
        for(int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j] = Instantiate(playerImages[j], new Vector2(middlePos[i].x, middlePos[i].y+(heightDistance*j)), Quaternion.identity) as GameObject;
                readyTexts[i, j] = selectionImages[i, j].transform.FindChild("Ready").gameObject;
                readyTexts[i, j].SetActive(false);
                selectionImages[i, j].transform.SetParent(canvas.transform);
                selectionImages[i, j].transform.localScale = scale;


            }


        }

        

        transformBack = new int[] { playerImages.Length - 1, playerImages.Length - 1, playerImages.Length - 1, playerImages.Length - 1 };

        spawnBirdman = GameObject.Find("SpawnAudioBirdman").GetComponent<MultipleAudioclips>();
        spawnTimeshifter = GameObject.Find("SpawnAudioTimeshifter").GetComponent<MultipleAudioclips>();
        spawnCharger = GameObject.Find("SpawnAudioCharger").GetComponent<MultipleAudioclips>();
        spawnFatman = GameObject.Find("SpawnAudioFatman").GetComponent<MultipleAudioclips>();
        spawnBabuschka = GameObject.Find("SpawnAudioBabuschka").GetComponent<MultipleAudioclips>();
        spawnPantomime = GameObject.Find("SpawnAudioPantomime").GetComponent<MultipleAudioclips>();


        // Register switch sound.
        ButtonSwitchEvent += PlaySwitchSound;
        ButtonAcceptEvent += PlayAcceptSound;
        ButtonDeclineEvent += PlayDeclineSound;


        slides = new GameObject[3];
        slides[0] = GameObject.Find("slide_1");

        skipText = slides[0].GetComponentInChildren<Text>();

        slides[1] = GameObject.Find("slide_2");

        slides[2] = GameObject.Find("slide_3");

        infoBar = GameObject.Find("InfoBar");

        gamerCountdown = GameObject.Find("GameStartsImage");
        //updateLanguage(slides[2]);

        skipText.enabled = false;
        slides[0].SetActive(false);
        slides[1].SetActive(false);
        slides[2].SetActive(false);




        originalVecInfo = infoBar.transform.localPosition;

        gamerCountdown.SetActive(false);
        gamerCountdownTime = 20.0f;

        //onceLevel = false;
        timeBeginning = 0.0f;
        first = true;


        imageColors = new Color[2];
        imageColors[0] = Color.white;
        imageColors[1] = new Color(0.3f,0.3f,0.3f);

        
        // GameObject.Find("GameName").GetComponent<Text>().text = network.gameName + " GAME";
    }

    bool changeButtonPosition()
    {

        for (int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for (int j = 0; j < selectionImages.GetLength(1); j++)
            {
                if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().back)
                {
                    selectionImages[i, j].transform.position = middlePos[i] + new Vector2(0,  heightDistance * j);
                    selectionImages[i, playerImages.Length - 1].transform.position = middlePos[i] + new Vector2(0,  -heightDistance);
                }
                else
                {
                    selectionImages[i, j].transform.position = middlePos[i] + new Vector2(0,  heightDistance * j);
                    selectionImages[i, playerImages.Length - 1].transform.position = middlePos[i] + new Vector2(0,  -heightDistance);
                }
            }
        }
        int count = 0;

        for(int i = 0; i < 4; i++)
        {
            if((int)selectionImages[i,1].transform.position.y != 0)
            {
                count++;
            } 
        }

        if(count == 4)
        {
            return true;
        }
        else
        {
            return false;
        }
       
    }

    private void HandleImages()
    {
       
        for (int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j].GetComponent<Image>().sprite = selectionImages[i, j].GetComponent<Button>().spriteState.disabledSprite;
                readyTexts[i, j].SetActive(false);
            }
           
            if (selectedCharacters[i]!=-1)
            {

                selectionImages[i, selectedCharacters[i]].GetComponent<Image>().sprite = selectionImages[i, selectedCharacters[i]].GetComponent<Button>().spriteState.highlightedSprite;
                readyTexts[i, selectedCharacters[i]].SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update () {

        if (!once)
        {
            if (changeButtonPosition())
            {               
                ScaleImages();
                once = true;
            }
        }

        UpdatePlayerStatus();
        
        HideInactiveElements();
        for (int i = 0; i < inputReceived.Length; i++)
        {
            if (inputReceived[i])
            {
                inputCooldown[i] -= Time.deltaTime;

                if (inputCooldown[i] <= 0.0f)
                {
                    inputCooldown[i] = inputCoolDownTime;
                    inputReceived[i] = false;
                }
            }
        }


        if (currentPlayerCount != 0)
        {            
            if (!CheckPlayerReady())
            {
                ResetImages();
                HandleInput();
                ProcessInput();
                HandleSelection();
                HandleImages();
                UpdateImageDisplay();
            }
            else
            {
                HideAll();
                Slides();
            }
        }
    }

    private void ResetImages()
    {
        for(int i=0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {

                selectionImages[i, j].GetComponent<Image>().color = imageColors[0];
            }
        }


    }

    private void HideAll()
    {
        for(int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j].SetActive(false);
                //selectionImages[i, j].transform.FindChild(selectionImages[i, j].name).gameObject.SetActive(false);
                readyTexts[i, j].SetActive(false);
            }
        }
    }
 
    private void ScaleImages()
    {
        for (int i = 0; i < selectionImages.GetLength(0); i++)
        {        
            
                              
            StartCoroutine(selectionImages[i, hoveredCharacters[i]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));
            
        }
    }



    

    private void UpdateImageDisplay()
    {
        for (int i = 0; i < selectedCharacters.Length; i++)
        {
            for(int j = 0; j < selectionImages.GetLength(0); j++)
            {
                for(int k = 0; k < selectionImages.GetLength(1); k++)
                {
                    if (i!= j && k == selectedCharacters[i])
                    {                        
                        selectionImages[j, k].GetComponent<Image>().color = imageColors[1];
                    }                    
                }
            }           
        }
    }




    private void HandleSelection()
    {
        for (int i = 0; i < selectionImages.GetLength(0); i++)
        {
            bool acceptSelection = true;
            int runningNumber = playerCursorSlot[i];
            

            string submit = "P" + (runningNumber+1) + "_Ability";

            for (int j = 0; j < selectedCharacters.Length; j++)
            {
                if (i != j && hoveredCharacters[i] == selectedCharacters[j])
                {
                    acceptSelection = false;
                }
            }

            if (runningNumber < 4 && runningNumber>=0 && Input.GetButtonDown(submit)&& acceptSelection)
            {                
                if (!playerSelected[i])
                {
                    selectedCharacters[i] = hoveredCharacters[i];
                    
                    if(hoveredCharacters[i] == selectionImages.GetLength(1) - 1)
                    {
                        OnButtonDeclined();
                        HandleBackButton();
                    }
                    else
                    {
                        PlaySpawnSound(hoveredCharacters[i]);                        
                    }                                   
                    playerSelected[i] = true;
                }
                else
                {
                    selectedCharacters[i] = -1;
                    playerSelected[i] = false;
                }
            }
            
            if(runningNumber>=4 && network.actionButton[runningNumber%4]==1 && acceptSelection)
            {                
                network.actionButton[runningNumber % 4] = 0;

                if (!playerSelected[i])
                {
                    selectedCharacters[i] = hoveredCharacters[i];

                    if (hoveredCharacters[i] == selectionImages.GetLength(1) - 1)
                    {
                        OnButtonDeclined();
                        HandleBackButton();
                    }
                    else
                    {
                        PlaySpawnSound(hoveredCharacters[i]);
                    }
                    playerSelected[i] = true;
                }
                else
                {
                    selectedCharacters[i] = -1;
                    playerSelected[i] = false;
                }
            }            
        }
    }




    protected void PlaySpawnSound(int character)
    {
        switch (character)
        {
            case 0:
                if (spawnBirdman != null)
                    spawnBirdman.PlayRandomClip();
                break;
            case 1:
                if (spawnCharger != null)
                    spawnCharger.PlayRandomClip();
                break;
            case 2:
                if (spawnFatman != null)
                    spawnFatman.PlayRandomClip();
                break;
            case 3:
                if (spawnTimeshifter != null)
                    spawnTimeshifter.PlayRandomClip();
                break;
            case 4:
                if (spawnBabuschka != null)
                    spawnBabuschka.PlayRandomClip();
                break;
            case 5:
                if (spawnPantomime != null)
                    spawnPantomime.PlayRandomClip();
                break;
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
                newPhone = true;
            }

        }

    }

    public void PlayerPhoneLeave(int slot)
    {
        playerSlotPhone[slot] = false;        
    }



    private void ProcessInput()
    {
        for(int i = 0; i < playerCursorSlot.Length; i++)
        {
            if(playerCursorSlot[i] != -1 && currentControllerInput[i] != -1)
            {
                if (!selectedChange[i])
                {                    
                    if (currentControllerInput[i] == 1 && selectedCharacters[i] == -1)
                    {                       
                        StartCoroutine(MoveUpwards(i));
                        selectedChange[i] = true;
                    }

                    if (currentControllerInput[i] == 3 && selectedCharacters[i] == -1)
                    {
                        StartCoroutine(MoveDownwards(i));
                        selectedChange[i] = true;
                    }
                }
            }
        }
    }



    private void HandleInput()
    {
        float x = 0.0f;
        float y = 0.0f;
        for (int i = 0; i < playerCursorSlot.Length; i++)
        {
            x = 0.0f;
            y = 0.0f;
            currentControllerInput[i] = -1;
            if (playerCursorSlot[i] < 4 && playerCursorSlot[i] > -1)
            {               
                int runningNumber = playerCursorSlot[i] + 1;

                string curHorizontal = "P" + runningNumber + "_Horizontal";
                string curVertical = "P" + runningNumber + "_Vertical";

                x = Input.GetAxisRaw(curHorizontal);
                y = Input.GetAxisRaw(curVertical);
                
            }

            else if (playerCursorSlot[i] >= 4)
            {              
                x = network.horizontal[playerCursorSlot[i] % 4];
                y = network.vertical[playerCursorSlot[i] % 4];
            }

            if (!inputReceived[i])
            {
                if (x < -0.5f)
                {
                    inputReceived[i] = true;
                    currentControllerInput[i] = 0;
                }
                else if (x > 0.5f)
                {
                    inputReceived[i] = true;
                    currentControllerInput[i] = 2;
                }
                else if (y < -0.5f)
                {
                    inputReceived[i] = true;
                    currentControllerInput[i] = 1;
                }
                else if (y > 0.5f)
                {
                    inputReceived[i] = true;
                    currentControllerInput[i] = 3;
                }
                else
                {
                    currentControllerInput[i] = -1;
                }
            }
        }
    }



    private void HandleBackButton()
    {
        if (!onceLevel)
        {
            if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().transitionFinished)
            {
                onceLevel = true;
                OnButtonDeclined();
                GameObject.Find("_StartMenu").GetComponent<StartMenu>().ChangeScenes("CharacterSelectionObjectNew(Clone)", "Scenes/Menu/LevelSelectionObject", true);
            }                
        }
    }




    private void UpdatePlayerStatus()
    {

        int newPhoneCount = 0;
        int newControlCount = 0;
        currentPlayerCount = 0;

        for (int i = 0; i < playerCursorSlot.Length; i++)
        {
            if (playerSlot[i])
            {
                newControlCount++;
                currentPlayerCount++;
            }

            if (playerSlotPhone[i])
            {
                newPhoneCount++;
                currentPlayerCount++;
            }
        }


        bool phoneAdded = false;
        if (newPhone)
        {
            for (int i = 0; i < playerCursorSlot.Length; i++)
            {
                if (!phoneAdded && playerCursorSlot[i] == -1)
                {

                    playerCursorSlot[i] = newPhoneCount + 3;                   
                    phoneAdded = true;
                    newPhone = false;
                }
            }
        }

        bool controllerAdded = false;
        if (newControlCount < Input.GetJoystickNames().Length && currentPlayerCount < 4)
        {
            playerSlot[Input.GetJoystickNames().Length - 1] = true;
            for (int i = 0; i < playerCursorSlot.Length; i++)
            {
                if (!controllerAdded && playerCursorSlot[i] == -1)
                {
                    playerCursorSlot[i] = Input.GetJoystickNames().Length - 1; ;                    
                    controllerAdded = true;
                }
            }
        }
    }

  
    private IEnumerator MoveUpwards(int index)
    {
        OnButtonSwitched();
        float y = 0;
         
        transformBack[index] --;

        if(transformBack[index] == -1)
        {
            transformBack[index] = playerImages.Length - 1;
        }


        selectionImages[index, transformBack[index]].transform.Translate(Vector3.down * heightDistance * playerImages.Length);
       
        
        StartCoroutine(selectionImages[index, hoveredCharacters[index]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.0f, 0.35f));
        hoveredCharacters[index]--;

        if (hoveredCharacters[index] == -1)
        {
            hoveredCharacters[index] = playerImages.Length - 1;
        }
        StartCoroutine(selectionImages[index, hoveredCharacters[index]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));

       

        while (y + Time.deltaTime * 2.6 < 1)
        {
            yield return 0;
            for (int i = 0; i < playerImages.Length; i++)
            {
                selectionImages[index, i].transform.Translate(Vector3.up * heightDistance * Time.deltaTime * 2.6f);
            }
            y += Time.deltaTime * 2.6f;
        }

        for(int i = 0; i < playerImages.Length; i++)
        {
            selectionImages[index, i].transform.Translate(Vector3.up * heightDistance * (1- y));
        }

        selectedChange[index] = false;
        currentControllerInput[index] = -1;
    }





    private IEnumerator MoveDownwards(int index)
    {
        OnButtonSwitched();
        float y = 0;

        StartCoroutine(selectionImages[index, hoveredCharacters[index]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.0f, 0.35f));

        hoveredCharacters[index] += 1;

        if(hoveredCharacters[index] == playerImages.Length)
        {
            hoveredCharacters[index] = 0;
        }
        
        StartCoroutine(selectionImages[index, hoveredCharacters[index]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));

        
       
        while (y + Time.deltaTime * 2.6 < 1)
        {
            yield return 0;
            for (int i = 0; i < playerImages.Length; i++)
            {
                selectionImages[index, i].transform.Translate(Vector3.down * heightDistance * Time.deltaTime * 2.6f);
            }
            y += Time.deltaTime * 2.6f;
        }

        for (int i = 0; i < playerImages.Length; i++)
        {
            selectionImages[index, i].transform.Translate(Vector3.down * heightDistance * (1 - y));
        }

        selectionImages[index,transformBack[index]].transform.Translate(Vector3.up * heightDistance * playerImages.Length);
        if (transformBack[index] == playerImages.Length - 1)
        {
            transformBack[index] = 0;
        }
        else
        {
            transformBack[index]++;
        }
        selectedChange[index] = false;
        currentControllerInput[index] = -1;
    }


    /// <summary>
    /// Loads the selected level asyncronously.
    /// </summary>
    protected IEnumerator asyncLoadLevel()
    {
        async = Application.LoadLevelAsync(levelInfo.levelIndex);
        async.allowSceneActivation = false;
       
     
        skipText.enabled = true;
        levelLoaded = true;
        yield return async;
    }



    protected IEnumerator WaitForAcceptSound()
    {
        OnButtonAccepted();

        //if (!GameObject.Find("slide_2").GetComponent<Image>().enabled)
        //{
        //    GameObject.Find("slide_2").GetComponent<Image>().enabled = true;
        //}
        //else if (!GameObject.Find("slide_3").GetComponent<Image>().enabled)
        //{
        //    GameObject.Find("slide_3").GetComponent<Image>().enabled = true;
        //}
        //else
        //{
        //    yield return new WaitForSeconds(acceptSound.length);
        //    CrushPolys();
        //}

        if (!slides[1].activeInHierarchy)
        {
            slides[1].SetActive(true);
        }
        else if (!slides[2].activeInHierarchy)
        {
            slides[2].SetActive(true);
        }
        else
        {
            yield return new WaitForSeconds(acceptSound.length);
            CrushPolys();
        }
    }



    private void Slides()
    {

        StorePlayerInformation();

        if (slides[0].activeInHierarchy != true)
        {
            slides[0].SetActive(true);
            StartCoroutine(asyncLoadLevel());
        }

        if (levelLoaded)
        {

            for (int i = 0; i < playerSlot.Length; i++)
            {
                int runningNumber = i + 1;
                string submit = "P" + runningNumber + "_Ability";
                if (playerSlot[i] == true && Input.GetButtonDown(submit))
                {

                    StartCoroutine(WaitForAcceptSound());
                    break;

                }
                else if (playerSlotPhone[i] == true && network.actionButton[i] == 1)
                {

                    for (int j = 0; j < 4; j++)
                    {
                        network.actionButton[j] = 0;
                    }

                    StartCoroutine(WaitForAcceptSound());
                    break;
                }
            }
        }

    }



    private void StorePlayerInformation()
    {
        if (levelInfo != null)
        {
            for (int i = 0; i < selectedCharacters.Length; i++)
            {
                if (playerCursorSlot[i] > -1 && playerCursorSlot[i] < 4)
                {
                    if (selectedCharacters[i] > -1)
                    {
                        levelInfo.playerSlot[playerCursorSlot[i]] = playerClassNames[selectedCharacters[i]];
                    }
                }
                else if (playerCursorSlot[i] >= 4)
                {
                    if (selectedCharacters[i] > -1)
                    {
                        levelInfo.phonePlayerSlot[playerCursorSlot[i] % 4] = playerClassNames[selectedCharacters[i]];
                    }
                }
            }
        }
    }



    /// <summary>
    /// Calls the CrushPolys function is every player has selected a char. Gets called every time if a player confirms his/her selection
    /// </summary>
    private bool CheckPlayerReady()
    {
        playerHasChosen = 0;

        for (int i = 0; i < selectedCharacters.Length; i++)
        {
            if (selectedCharacters[i] >= 0 || selectedCharacters[i] == -2)
            {
                playerHasChosen++;
            }

        }

        if (playerHasChosen == 0)
        {
            gamerCountdown.SetActive(false);
            infoBar.transform.localPosition = originalVecInfo;
            first = true;
        }

        if (gamerCountdownTime <= 0f)
        {

            gamerCountdown.SetActive(false);
            infoBar.transform.localPosition = originalVecInfo;
            first = true;

            return true;

        }

        if (playerHasChosen > 0)
        {

            if (first)
            {
                timeBeginning = Time.time;
                gamerCountdownTime = 20.0f;
                gamerCountdown.SetActive(true);
                infoBar.transform.localPosition = new Vector3(infoBar.transform.localPosition.x, -200, 0);
                first = false;
            }
            else
            {
                gamerCountdownTime = 20 - (Time.time - timeBeginning);
                GameObject.Find("GameStartsText").GetComponent<Text>().text = ((int)(gamerCountdownTime)).ToString();
            }

        }

        if (currentPlayerCount == playerHasChosen && gamerCountdownTime > 4f)
        {
            timeBeginning -= gamerCountdownTime - 4f;
        }

        return false;
    }

    private void HideInactiveElements()
    {
        for(int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j].SetActive(true);
            }
        }

        for(int i = selectionImages.GetLength(0)-1; i > currentPlayerCount-1; i--)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j].SetActive(false);
            }
        }

    }




    /// <summary>
    /// Starts the selected and preloaded level and destroys the menu music.
    /// </summary>
    public void CrushPolys()
    {
        async.allowSceneActivation = true;

        GameObject music = GameObject.Find("MenuMusic");

        if (music != null)
            Destroy(music);

      
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




    protected void SetCoundownFalse()
    {
        //zero = false;
        //one = false;
        //two = false;
        //three = false;
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
}
