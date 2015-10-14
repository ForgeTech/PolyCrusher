using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Menu script for the character selection menu.
/// </summary>
public class CharacterMenu : MonoBehaviour
{
    //Reference tho the level information
    public LevelStartInformation levelInfo;

    //public PlayerManager playerManager;

    public PlayerNetCommunicate network;

    public int currentPlayerCount;

    //The player slots of the four players. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlot = new bool[4];

    //The player slots of the four players for the phone. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlotPhone = new bool[4];

    public GameObject[] playerTypes = new GameObject[4];
    public GameObject[] playerCursor = new GameObject[4];
    public Image[] playerStatus = new Image[4];
    public Image[] charImages = new Image[4];
    public Image[] charMargins = new Image[4];
    //public ParticleSystem[] particles = new ParticleSystem[4];
    public int[] playerCursorSlot;

    public Image[] backButton = new Image[2];

    private string[] playerClassNames;

    public float width = Screen.width;
    public float height = Screen.height;

    public float widthStart;
    public float heightStart;

    public float widthStep;
    public float heightStep;

    public float xMargin = 12.0f;
    public float yMargin = 30.0f;

    public float backDistance = 150.0f;

    public float timeRemaining = 4f;

    public float speed = 0.0f;

    //used to store the selection status - 0 - nothing, 1 - hover, 2 - selected 
    public int[] charSelectionStatus = new int[4];

    //which cursor is hovering over the playerType Images
    public int[] currentHoveredChar = new int[4];

    private int playerCount;

    private bool startChecking = false;

    public GameObject[] playerImages = new GameObject[4];


    public Color[] cursorColors;

    public Image[] playerText;

    public int[] playerSelection;

    bool newPhone = false;

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


    //Delegates
    private event ButtonSwitchedEvent ButtonSwitchEvent;
    private event ButtonAcceptedEvent ButtonAcceptEvent;
    private event ButtonDeclinedEvent ButtonDeclineEvent;


    //async loading
    private AsyncOperation async;
    private bool levelLoaded = false;
    private bool isLoading = false;

    //slides
    private bool show_once = false;

    //countdown bools
    private bool zero = false;
    private bool one = false;
    private bool two = false;
    private bool three = false;

    private bool once = true;
    bool onceLevel = false;

    int playerHasChosen = 0;

    GameObject infoBar;
    GameObject gamerCountdown;

    Vector3 originalVecCountdown;
    Vector3 originalVecInfo;

    float gamerCountdownTime;
    float timeBeginning;
    bool first;

    Vector3 originalScaleGameStartsText;

    void Awake()
    {
        spawnBirdman = GameObject.Find("SpawnAudioBirdman").GetComponent<MultipleAudioclips>();
        spawnTimeshifter = GameObject.Find("SpawnAudioTimeshifter").GetComponent<MultipleAudioclips>();
        spawnCharger = GameObject.Find("SpawnAudioCharger").GetComponent<MultipleAudioclips>();
        spawnFatman = GameObject.Find("SpawnAudioFatman").GetComponent<MultipleAudioclips>();

        // Register switch sound.
        ButtonSwitchEvent += PlaySwitchSound;
        ButtonAcceptEvent += PlayAcceptSound;
        ButtonDeclineEvent += PlayDeclineSound;

        //find slides and disable it
        GameObject.Find("slide_1").GetComponent<Image>().enabled = false;
        GameObject.Find("slide_2").GetComponent<Image>().enabled = false;
        GameObject.Find("slide_3").GetComponent<Image>().enabled = false;
        GameObject.Find("slide_text").GetComponent<Image>().enabled = false;

        infoBar = GameObject.Find("InfoBar");
        gamerCountdown = GameObject.Find("GameStartsImage");

        originalVecCountdown = gamerCountdown.transform.localPosition;
        originalVecInfo = infoBar.transform.localPosition;

        gamerCountdown.SetActive(false);
        gamerCountdownTime = 20.0f;

        onceLevel = false;
        timeBeginning = 0.0f;
        first = true;

        Debug.Log(Screen.width + " " + Screen.height);

        widthStep = Screen.width / 5.0f * 1.1f;
        heightStep = Screen.height / 3.0f;

        widthStart = Screen.width / 7.0f * 1.20f;
        heightStart = Screen.height / 4.0f * 1.8f;
        Debug.Log(widthStep + " " + heightStep);
    }

    // Use this for initialization
    public void Start()
    {

        playerCursorSlot = new int[4] { -1, -1, -1, -1 };

        for (int i = 0; i < charImages.Length; i++)
        {
            charImages[i] = playerTypes[i].GetComponent<Image>();
        }

        network = GameObject.FindObjectOfType<PlayerNetCommunicate>();
        GameObject.Find("GameNameCharacterSelection").GetComponent<Text>().text = network.gameName + " GAME";

        playerSelection = new int[4] { -1, -1, -1, -1 };
        currentHoveredChar = new int[4] { -1, -1, -1, -1 };
        cursorColors = new Color[4] { Color.red, Color.cyan, Color.yellow, Color.white };
        playerClassNames = new string[4] { "Birdman", "Charger", "Fatman", "Timeshifter" };

        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
        levelInfo.ClearPlayerArrays();

        playerSlotPhone = new bool[4];

        playerCount = Input.GetJoystickNames().Length;
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
                playerCursorSlot[playerCount + i] = 5 + i;
                playerSlotPhone[i] = true;
                currentPlayerCount++;

            }
        }

        StartCoroutine(ScalePlayerImages());

        for (int i = 0; i < playerCursorSlot.Length; i++)
        {
            if (playerCursorSlot[i] != -1)
            {
                playerCursor[i].GetComponent<Image>().enabled = false;
                playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().enabled = false;
            }
        }

        originalScaleGameStartsText = gamerCountdown.transform.localScale;


    }

    IEnumerator ScalePlayerImages()
    {



        //playerImages[0] = GameObject.Find("Birdman");
        //playerImages[1] = GameObject.Find("Charger");
        //playerImages[2] = GameObject.Find("Fatman");
        //playerImages[3] = GameObject.Find("TimeShifter");

        //for (int i = 0; i < playerImages.Length; i++) {
        //	playerImages[i].GetComponent<Image>().enabled = false;
        //}

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < playerImages.Length; i++)
        {
            Vector3 originalScale = playerImages[i].transform.localScale;
            playerImages[i].transform.localScale = Vector3.zero;
            playerImages[i].GetComponent<Image>().enabled = true;
            StartCoroutine(playerImages[i].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));

            yield return new WaitForSeconds(0.3f);
        }

        startChecking = true;
    }

    void Update()
    {

        UpdatePlayerStatus();
        PlayCountdownSounds();

        if (currentPlayerCount != 0)
        {
            if (!CheckPlayerReady())
            {
                HandleMovement();
                HandleSelection();
                CheckHover();
                HandleBackButton();
                RefreshImages();

            }
            else
            {
                Slides();
            }

        }

    }

    void PlayCountdownSounds()
    {
        if (((int)gamerCountdownTime) == 3 && !three)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);

            gamerCountdown.transform.localScale = originalScaleGameStartsText;
            gamerCountdown.transform.localScale *= 1.2f;
            StartCoroutine(gamerCountdown.transform.ScaleTo(originalScaleGameStartsText, 0.2f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            three = true;
        }
        if (((int)gamerCountdownTime) == 2 && !two)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);

            gamerCountdown.transform.localScale = originalScaleGameStartsText;
            gamerCountdown.transform.localScale *= 1.2f;
            StartCoroutine(gamerCountdown.transform.ScaleTo(originalScaleGameStartsText, 0.2f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            two = true;
        }
        if (((int)gamerCountdownTime) == 1 && !one)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);

            gamerCountdown.transform.localScale = originalScaleGameStartsText;
            gamerCountdown.transform.localScale *= 1.2f;
            StartCoroutine(gamerCountdown.transform.ScaleTo(originalScaleGameStartsText, 0.2f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            one = true;
        }
        if (((int)gamerCountdownTime) == 0 && !zero)
        {
            SoundManager.SoundManagerInstance.Play(zeroSound, Vector3.zero);

            gamerCountdown.transform.localScale = originalScaleGameStartsText;
            gamerCountdown.transform.localScale *= 1.2f;
            StartCoroutine(gamerCountdown.transform.ScaleTo(originalScaleGameStartsText, 0.2f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            zero = true;
        }
    }



    private void Slides()
    {

        StorePlayerInformation();

        if (GameObject.Find("slide_1").GetComponent<Image>().enabled == false)
        {
            GameObject.Find("slide_1").GetComponent<Image>().enabled = true;
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

    private void HandleBackButton()
    {

        int counter = 0;

        for (int i = 0; i < playerCursor.Length; i++)
        {
            if (CheckBackButtonHover(i))
            {

                //backChanged = true;
                backButton[1].gameObject.SetActive(true);
                backButton[0].gameObject.SetActive(false);

                int runningNumber = i + 1;
                string curSubmit = "P" + runningNumber + "_Ability";

                if (playerCursorSlot[i] > -1 && playerCursorSlot[i] < 5 && Input.GetButtonDown(curSubmit))
                {
                    if (!onceLevel)
                    {
                        if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().transitionFinished)
                        {
                            onceLevel = true;
                            OnButtonDeclined();
                            GameObject.Find("_StartMenu").GetComponent<StartMenu>().ChangeScenes("CharacterSelectionObject(Clone)", "Scenes/Menu/LevelSelectionObject", true);
                        }
                    }

                }
                else if (playerCursorSlot[i] > 4 && network.actionButton[playerCursorSlot[i] % 5] == 1)
                {
                    if (network.actionButton[playerCursorSlot[i] % 5] == 1)
                    {
                        network.actionButton[playerCursorSlot[i] % 5] = 0;
                        network.sendData("1004", playerCursorSlot[i] % 5);
                    }

                    if (!onceLevel)
                    {
                        if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().transitionFinished)
                        {
                            onceLevel = true;
                            OnButtonDeclined();
                            GameObject.Find("_StartMenu").GetComponent<StartMenu>().ChangeScenes("CharacterSelectionObject(Clone)", "Scenes/Menu/LevelSelectionObject", true);
                        }
                    }
                }


            }
            else
            {
                counter++;
            }
        }

        if (counter == playerCursor.Length)
        {
            backButton[1].gameObject.SetActive(false);
            backButton[0].gameObject.SetActive(true);
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
            if (!playerSlotPhone[slot])
            {

                playerSlotPhone[slot] = true;
                levelInfo.phonePlayerSlotTaken[slot] = true;
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

        for (int i = 0; i < playerCursorSlot.Length; i++)
        {
            if (playerCursorSlot[i] == slot + 5)
            {

                //playerCursorSlot[i] = -1;
                //playerText[i].gameObject.SetActive(false);
                //playerText[i].transform.position = new Vector2(-1200, playerText[i].transform.position.y);
                playerCursor[i].GetComponent<Image>().enabled = false;
                playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().enabled = false;


            }
        }
    }

    void ScalePlayerCursorImage(int i)
    {

        Vector3 originalScale = playerCursor[i].transform.localScale;
        playerCursor[i].transform.localScale = Vector3.zero;
        playerCursor[i].GetComponent<Image>().enabled = true;
        playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().enabled = true;
        StartCoroutine(playerCursor[i].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));

    }

    private void UpdatePlayerStatus()
    {

        int newPhoneCount = 0;
        int newControlCount = 0;
        currentPlayerCount = 0;

        for (int i = 0; i < playerCursor.Length; i++)
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

                    playerCursorSlot[i] = newPhoneCount + 4;
                    playerCursor[i].SetActive(true);
                    playerCursor[i].GetComponent<Image>().enabled = false;
                    playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().enabled = false;
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
                    playerCursor[i].SetActive(true);
                    playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().enabled = false;
                    controllerAdded = true;
                }
            }
        }

        for (int i = 0; i < playerCursorSlot.Length; i++)
        {
            if (playerCursorSlot[i] == -1)
            {
                playerCursor[i].SetActive(false);
            }
        }

        bool change = false;

        for (int i = 0; i < playerCursor.Length; i++)
        {
            if (playerCursorSlot[i] < 5 && playerCursorSlot[i] > -1)
            {

                int runningNumber = playerCursorSlot[i] + 1;

                string curHorizontal = "P" + runningNumber + "_Horizontal";
                string curVertical = "P" + runningNumber + "_Vertical";

                if (Input.GetAxis(curHorizontal) <= -0.2f || Input.GetAxis(curHorizontal) >= 0.2f)
                {
                    if (playerCursor[i].GetComponent<Image>().enabled == false)
                    {
                        ScalePlayerCursorImage(i);
                        change = true;
                    }
                }
                else if (Input.GetAxis(curVertical) <= -0.2f || Input.GetAxis(curVertical) >= 0.2f)
                {
                    if (playerCursor[i].GetComponent<Image>().enabled == false)
                    {
                        ScalePlayerCursorImage(i);
                        change = true;
                    }
                }
            }

            else if (playerCursorSlot[i] > 4)
            {

                if (network.horizontal[playerCursorSlot[i] % 5] <= -0.2f || network.horizontal[playerCursorSlot[i] % 5] >= 0.2f)
                {
                    if (playerCursor[i].GetComponent<Image>().enabled == false)
                    {
                        ScalePlayerCursorImage(i);
                        change = true;
                    }
                }
                else if (network.vertical[playerCursorSlot[i] % 5] <= -0.2f || network.vertical[playerCursorSlot[i] % 5] >= 0.2f)
                {
                    if (playerCursor[i].GetComponent<Image>().enabled == false)
                    {
                        ScalePlayerCursorImage(i);
                        change = true;
                    }
                }
            }
        }

        int playerNumberString = 0;

        if (change)
        {
            for (int i = 0; i < playerCursor.Length; i++)
            {
                if (playerCursor[i].GetComponent<Image>().enabled == true)
                {
                    playerNumberString++;
                    playerCursor[i].transform.Find("PlayerNumberText").GetComponent<Text>().text = playerNumberString.ToString();

                    if (playerNumberString == 1)
                    {
                        playerText[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/1");
                    }
                    else if (playerNumberString == 2)
                    {
                        playerText[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/2");
                    }
                    else if (playerNumberString == 3)
                    {
                        playerText[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/3");
                    }
                    else if (playerNumberString == 4)
                    {
                        playerText[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Menu/CharacterSelectionMenu/4");
                    }
                }
            }
        }

    }


    /*private void Countdown()
    {

        if (timeRemaining > 0.0f)
        {
			Image countdownImage = GameObject.Find ("Countdown").GetComponent<Image>();

			if (countdownImage.enabled == false) {
				countdownImage.transform.localScale = Vector3.zero;
				countdownImage.enabled = true;
				StartCoroutine(countdownImage.transform.ScaleTo(new Vector3(1,1,1), 0.3f, AnimCurveContainer.AnimCurve.grow.Evaluate));
				once = false;
			}
			
			timeRemaining -= Time.deltaTime;

			if (timeRemaining < 3f && !once) {

					countdownImage.transform.localScale = Vector3.zero;
					countdownImage.sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/countdown_2");
					StartCoroutine(countdownImage.transform.ScaleTo(new Vector3(1,1,1), 0.2f, AnimCurveContainer.AnimCurve.grow.Evaluate));
					once = true;

			} else if (((int)timeRemaining) == 1) {

                countdownImage.transform.localScale = Vector3.zero;
				countdownImage.sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/countdown_1");
				StartCoroutine(countdownImage.transform.ScaleTo(new Vector3(1,1,1), 0.2f, AnimCurveContainer.AnimCurve.grow.Evaluate));

			}

        }
        else
        {
			GameObject.Find ("Countdown").GetComponent<Image>().enabled = false;
        }

        if (((int)timeRemaining) == 3 && !three)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);
            three = true;
        }
        if (((int)timeRemaining) == 2 && !two)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);
            two = true;
        }
        if (((int)timeRemaining) == 1 && !one)
        {
            SoundManager.SoundManagerInstance.Play(beepSound, Vector3.zero);
            one = true;
        }
        if (((int)timeRemaining) == 0 && !zero)
        {
            SoundManager.SoundManagerInstance.Play(zeroSound, Vector3.zero);
            zero = true;
        }
    }*/


    private void StorePlayerInformation()
    {
        if (levelInfo != null)
        {
            for (int i = 0; i < playerSelection.Length; i++)
            {
                if (playerCursorSlot[i] > -1 && playerCursorSlot[i] < 5)
                {
                    if (playerSelection[i] > -1)
                    {
                        levelInfo.playerSlot[playerCursorSlot[i]] = playerClassNames[playerSelection[i]];
                    }
                }
                else if (playerCursorSlot[i] > 4)
                {
                    if (playerSelection[i] > -1)
                    {
                        levelInfo.phonePlayerSlot[playerCursorSlot[i] % 5] = playerClassNames[playerSelection[i]];
                    }
                }
            }
        }

    }

    private void HandleSelection()
    {
        bool[] changed = new bool[4];

        for (int i = 0; i < playerCursor.Length; i++)
        {

            int runningNumber = i + 1;
            string curSubmit = "P" + runningNumber + "_Ability";
            for (int j = 0; j < playerSelection.Length; j++)
            {


                if (playerCursorSlot[i] > -1 && playerCursorSlot[i] < 5 && Input.GetButtonDown(curSubmit) && playerSelection[i] == -1 && currentHoveredChar[i] == j && charSelectionStatus[j] == 1)
                {
                    PlaySpawnSound(j);
                    SetCoundownFalse();
                    playerSelection[i] = j;
                    charSelectionStatus[j] = 2;

                    playerText[i].transform.position = new Vector2(playerTypes[j].transform.position.x, playerText[j].transform.position.y);
                    playerText[i].gameObject.SetActive(true);

                    playerCursor[i].SetActive(false);
                    changed[i] = true;

                }
                else if (playerCursorSlot[i] > 4 && network.actionButton[playerCursorSlot[i] % 5] == 1 && playerSelection[i] == -1 && currentHoveredChar[i] == j && charSelectionStatus[j] == 1)
                {
                    PlaySpawnSound(j);
                    SetCoundownFalse();
                    if (network.actionButton[playerCursorSlot[i] % 5] == 1)
                    {
                        network.actionButton[playerCursorSlot[i] % 5] = 0;
                        network.sendData("1004", playerCursorSlot[i] % 5);
                    }
                    playerSelection[i] = j;
                    charSelectionStatus[j] = 2;
                    playerText[i].transform.position = new Vector2(playerTypes[j].transform.position.x, playerText[i].transform.position.y);
                    playerText[i].gameObject.SetActive(true);
                    playerCursor[i].SetActive(false);
                    changed[i] = true;

                }

            }

            if (changed[i] == false)
            {

                if (playerCursorSlot[i] < 5 && Input.GetButtonDown(curSubmit) && playerSelection[i] >= 0)
                {
                    playerCursor[i].SetActive(true);
                    playerCursor[i].transform.position = new Vector3(charMargins[playerSelection[i]].transform.position.x, charMargins[playerSelection[i]].transform.position.y - height / 2, 0);

                    playerText[i].gameObject.SetActive(false);
                    playerText[i].transform.position = new Vector2(-1200, playerText[i].transform.position.y);

                    charSelectionStatus[playerSelection[i]] = 0;
                    playerSelection[i] = -1;

                }
                else if (playerCursorSlot[i] > 4 && network.actionButton[playerCursorSlot[i] % 5] == 1 && playerSelection[i] >= 0)
                {

                    if (network.actionButton[playerCursorSlot[i] % 5] == 1)
                    {
                        network.actionButton[playerCursorSlot[i] % 5] = 0;
                        network.sendData("1004", playerCursorSlot[i] % 5);
                    }

                    playerCursor[i].SetActive(true);
                    playerCursor[i].transform.position = new Vector3(charMargins[playerSelection[i]].transform.position.x, charMargins[playerSelection[i]].transform.position.y - height / 2, 0);

                    playerText[i].gameObject.SetActive(false);
                    playerText[i].transform.position = new Vector2(-1200, playerText[i].transform.position.y);

                    charSelectionStatus[playerSelection[i]] = 0;
                    playerSelection[i] = -1;

                }
            }
        }
    }

    private bool CheckBackButtonHover(int index)
    {
        if (Vector3.Distance(playerCursor[index].transform.position, backButton[0].transform.position) < backDistance)
        {
            return true;
        }
        return false;
    }


    private void HandleMovement()
    {
        for (int i = 0; i < playerCursor.Length; i++)
        {
            if (playerCursorSlot[i] < 5 && playerCursorSlot[i] > -1)
            {
                float x = 0.0f;
                float y = 0.0f;
                int runningNumber = playerCursorSlot[i] + 1;

                string curHorizontal = "P" + runningNumber + "_Horizontal";
                string curVertical = "P" + runningNumber + "_Vertical";

                if (Input.GetAxis(curHorizontal) <= -0.2f || Input.GetAxis(curHorizontal) >= 0.2f)
                {
                    x = Input.GetAxisRaw(curHorizontal) * speed * Time.deltaTime;

                }
                if (Input.GetAxis(curVertical) <= -0.2f || Input.GetAxis(curVertical) >= 0.2f)
                {
                    y = Input.GetAxisRaw(curVertical) * -speed * Time.deltaTime;
                }


                if (playerCursor[i].transform.position.x >= 0.0f && playerCursor[i].transform.position.x <= Screen.width && playerCursor[i].transform.position.y >= 0.0f && playerCursor[i].transform.position.y <= Screen.height)
                {
                    playerCursor[i].transform.Translate(new Vector3(x, y, 0.0f));

                }
                else
                {
                    if (playerCursor[i].transform.position.x < 0.0f)
                    {
                        playerCursor[i].transform.position = new Vector2(Screen.width, playerCursor[i].transform.position.y);
                    }

                    if (playerCursor[i].transform.position.x > Screen.width)
                    {
                        playerCursor[i].transform.position = new Vector2(0.0f, playerCursor[i].transform.position.y);
                    }

                    if (playerCursor[i].transform.position.y < 0.0f)
                    {
                        playerCursor[i].transform.position = new Vector2(playerCursor[i].transform.position.x, playerCursor[i].transform.position.y + Screen.height);
                    }

                    if (playerCursor[i].transform.position.y > Screen.height)
                    {
                        playerCursor[i].transform.position = new Vector2(playerCursor[i].transform.position.x, playerCursor[i].transform.position.y - Screen.height);
                    }

                }


            }

            else if (playerCursorSlot[i] > 4)
            {
                float x = 0.0f;
                float y = 0.0f;


                x = network.horizontal[playerCursorSlot[i] % 5] * speed * Time.deltaTime;

                //if (network.vertical[playerCursorSlot[i]%5] <= -0.2f || network.vertical[playerCursorSlot[i]%5] >= 0.2f)
                //{
                y = network.vertical[playerCursorSlot[i] % 5] * -speed * Time.deltaTime;
                //}

                if (playerCursor[i].transform.position.x >= 0.0f && playerCursor[i].transform.position.x <= Screen.width && playerCursor[i].transform.position.y >= 0.0f && playerCursor[i].transform.position.y <= Screen.height)
                {
                    playerCursor[i].transform.Translate(new Vector3(x, y, 0.0f));

                }
                else
                {
                    if (playerCursor[i].transform.position.x < 0.0f)
                    {
                        playerCursor[i].transform.position = new Vector2(Screen.width, playerCursor[i].transform.position.y);
                    }

                    if (playerCursor[i].transform.position.x > Screen.width)
                    {
                        playerCursor[i].transform.position = new Vector2(0.0f, playerCursor[i].transform.position.y);
                    }

                    if (playerCursor[i].transform.position.y < 0.0f)
                    {
                        playerCursor[i].transform.position = new Vector2(playerCursor[i].transform.position.x, Screen.height);
                    }

                    if (playerCursor[i].transform.position.y > Screen.height)
                    {
                        playerCursor[i].transform.position = new Vector2(playerCursor[i].transform.position.x, 0.0f);
                    }

                }

            }

        }


    }

    private void RefreshImages()
    {
        for (int i = 0; i < charImages.Length; i++)
        {
            if (charSelectionStatus[i] == 0)
            {

                charMargins[i].gameObject.SetActive(false);
                charImages[i].gameObject.SetActive(true);
            }
            else if (charSelectionStatus[i] == 1)
            {

                charMargins[i].gameObject.SetActive(true);
                charImages[i].gameObject.SetActive(false);
            }
            else if (charSelectionStatus[i] == 2)
            {
                //charImages[i].color = Color.grey;

            }
            if (playerSelection[i] != -1 && playerSelection[i] != -2)
            {
                //charMargins[playerSelection[i]].color = cursorColors[i];
            }

            if (playerSelection[i] == -2)
            {
                // playerText[i].text = "";
            }

            if (playerCursorSlot[i] != -1)
            {
                playerStatus[i].gameObject.SetActive(true);

            }
            else
            {

                playerStatus[i].gameObject.SetActive(false);
            }

        }

    }

    //private void CheckHover()
    //{
    //    bool[] changed = new bool[4];
    //    bool[] currentChange = new bool[4];
    //    for (int i = 0; i < playerCursor.Length; i++)
    //    {
    //        if (playerCursor[i].activeInHierarchy)
    //        {
    //            for (int j = 0; j < playerTypes.Length; j++)
    //            {
    //                if ((playerTypes[j].transform.position.x - (width / 2)) <= playerCursor[i].transform.position.x && (playerTypes[j].transform.position.x + (width / 2)) >= playerCursor[i].transform.position.x
    //                   && (playerTypes[j].transform.position.y - (height / 2)) <= playerCursor[i].transform.position.y && (playerTypes[j].transform.position.y + (height / 2)) >= playerCursor[i].transform.position.y)
    //                {

    //                    if (charSelectionStatus[j] != 2)
    //                    {
    //                        currentHoveredChar[i] = j;
    //                        currentChange[i] = true;

    //                        charSelectionStatus[j] = 1;
    //                        changed[j] = true;

    //                    }

    //                }
    //                if (currentChange[i] == false)
    //                {
    //                    currentHoveredChar[i] = -1;
    //                }

    //                if (changed[j] == false)
    //                {


    //                    if (charSelectionStatus[j] == 1)
    //                    {

    //                        charSelectionStatus[j] = 0;
    //                    }


    //                } 
    //}   
    //        }
    //    }
    //}



    private void CheckHover()
    {
        if (!startChecking)
        {
            return;
        }
        bool[] changed = new bool[4];
        bool[] currentChange = new bool[4];
        for (int i = 0; i < playerCursor.Length; i++)
        {
            if (playerCursor[i].activeInHierarchy)
            {
                for (int j = 0; j < playerTypes.Length; j++)
                {
                    if ((widthStart + widthStep * (j) - widthStep / 2) <= playerCursor[i].transform.position.x && ((widthStart + widthStep * (j) + widthStep / 2)) >= playerCursor[i].transform.position.x
                       && (heightStart - (heightStep / 2)) <= playerCursor[i].transform.position.y && (heightStart + (heightStep / 2)) >= playerCursor[i].transform.position.y)
                    {

                        if (charSelectionStatus[j] != 2)
                        {
                            currentHoveredChar[i] = j;
                            currentChange[i] = true;

                            charSelectionStatus[j] = 1;
                            changed[j] = true;

                        }

                    }
                    if (currentChange[i] == false)
                    {
                        currentHoveredChar[i] = -1;
                    }

                    if (changed[j] == false)
                    {


                        if (charSelectionStatus[j] == 1)
                        {

                            charSelectionStatus[j] = 0;
                        }


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

        for (int i = 0; i < charSelectionStatus.Length; i++)
        {
            if (playerSelection[i] >= 0 || playerSelection[i] == -2)
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

    /// <summary>
    /// Starts the selected and preloaded level and destroys the menu music.
    /// </summary>
    public void CrushPolys()
    {
        async.allowSceneActivation = true;

        GameObject music = GameObject.Find("MenuMusic");

        if (music != null)
            Destroy(music);

        Debug.Log("level start activation");
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

    /// <summary>
    /// Loads the selected level asyncronously.
    /// </summary>
    protected IEnumerator asyncLoadLevel()
    {
        async = Application.LoadLevelAsync(levelInfo.levelIndex);
        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
        {
            //Debug.Log(async.progress);
        }
        Debug.Log("level loaded 90%");
        GameObject.Find("slide_text").GetComponent<Image>().enabled = true;
        levelLoaded = true;
        yield return async;
    }

    protected IEnumerator WaitForAcceptSound()
    {
        OnButtonAccepted();

        if (!GameObject.Find("slide_2").GetComponent<Image>().enabled)
        {
            GameObject.Find("slide_2").GetComponent<Image>().enabled = true;
        }
        else if (!GameObject.Find("slide_3").GetComponent<Image>().enabled)
        {
            GameObject.Find("slide_3").GetComponent<Image>().enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(acceptSound.length);
            CrushPolys();
        }
    }

    protected void SetCoundownFalse()
    {
        zero = false;
        one = false;
        two = false;
        three = false;
    }

    protected void PlaySpawnSound(int character)
    {
        switch (character)
        {
            case 0:
                if (spawnTimeshifter != null)
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
                if (spawnBirdman != null)
                    spawnTimeshifter.PlayRandomClip();
                break;
        }
    }
}
