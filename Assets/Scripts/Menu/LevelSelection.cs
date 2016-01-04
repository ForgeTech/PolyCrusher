using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Level script for the level selection.
/// </summary>
public class LevelSelection : MonoBehaviour 
{
    //Reference tho the level information
    private LevelStartInformation levelInfo;

    private int maxPlayers;


    private bool moveLeft = false;
    private bool moveRight = false;

	private bool selectedChange = false;
    public float inputCooldownTime;

    private Vector2 middlePos;
    private float distance;



    private int playerCount;
  
    public PlayerNetCommunicate network;

    //The player slots of the four players. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlot = new bool[4];

    //The player slots of the four players for the phone. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlotPhone = new bool[4];

    public int currentPlayerCount;
    //bool newPhone = false;


    //private float animationTime = 0.0f;
    private float inputCooldown = 0.2f;

    private bool inputReceived = false;

    public GameObject[] buttons;
	public int selected;

    //Hard coded level indices for max. 5 levels
    public int[] levelIndices = new int[5];

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

	public bool finishedLoading = false;

	bool onceScale;
	bool onceLevel;

	int levelCounter;
	int levelTransformBack;

	bool levelScaled;

    void Awake()
    {
        GameObject music = GameObject.Find("MenuMusic");

        if (music != null)
            DontDestroyOnLoad(music);

        ButtonSwitchEvent += PlaySwitchSound;
        ButtonAcceptEvent += PlayAcceptSound;
        ButtonDeclineEvent += PlayDeclineSound;

		finishedLoading = false;
		onceScale = false;
		onceLevel = false;

		
		levelScaled = false;
	
	}

    void Start()
    {

		buttons = new GameObject[7];

		buttons[0] = GameObject.Find("Select_Level_01");
		buttons[1] = GameObject.Find("Select_Level_02");
		buttons[2] = GameObject.Find("Select_Level_03");
		buttons[3] = GameObject.Find("Select_Level_04");
        buttons[4] = GameObject.Find("Select_Level_05");
		buttons[5] = GameObject.Find("Select_Level_06");
        buttons[6] = GameObject.Find("Select_Main_Menu");

        levelTransformBack = buttons.Length - 1;
        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
		selected =0;

		middlePos = new Vector2(Screen.width/2, Screen.height/2 - Screen.height/18);

        distance = Screen.width - middlePos.x;

        Debug.Log(distance);

        //buttonSize = Screen.height / 2.7f;

        maxPlayers = 4;

        inputCooldown = inputCooldownTime;

        network = GameObject.FindObjectOfType<PlayerNetCommunicate>();

        playerSlotPhone = new bool[4];

		GameObject.Find("GameNameLevelSelection").GetComponent<Text>().text = network.gameName + " GAME";

        playerCount = Input.GetJoystickNames().Length;
        currentPlayerCount = playerCount;

        Debug.Log(playerCount);

		oldCounter = 0;

        for (int i = 0; i < playerCount; i++)
        {

            playerSlot[i] = true;
            levelInfo.playerSlotTaken[i] = true;

        }

        for (int i = 0; i < levelInfo.phonePlayerSlotTaken.Length; i++)
        {
			//Debug.Log (levelInfo.phonePlayerSlotTaken[0] + " " + levelInfo.phonePlayerSlotTaken[1] + " " +levelInfo.phonePlayerSlotTaken[2] + " " +levelInfo.phonePlayerSlotTaken[3] + " ");
            if (levelInfo.phonePlayerSlotTaken[i] && playerCount < 4)
            {
                playerSlotPhone[i] = true;
                currentPlayerCount++;

            }
        }

        levelIndices = new int[6];

		levelIndices [0] = 1;
		levelIndices [1] = 2;
		levelIndices [2] = 3;
		levelIndices [3] = 4;
		levelIndices [4] = 5;
        levelIndices [5] = 6;

        for (int i = 0; i < buttons.Length; i++) {
			buttons[i].GetComponent<Image>().enabled = false;
		}

    }

	bool changeButtonPosition() {

		for (int i = 0; i < buttons.Length; i++) {

			if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().back) {
				buttons[i].transform.position = middlePos + new Vector2(distance * i, -Screen.height);
				buttons[buttons.Length - 1].transform.position = middlePos + new Vector2(-distance, -Screen.height);
			} else {
				buttons[i].transform.position = middlePos + new Vector2(distance * i, Screen.height);
				buttons[buttons.Length - 1].transform.position = middlePos + new Vector2(-distance, Screen.height);
			}
		}

		if ((int) buttons [1].transform.position.x != 0) {
			return true;
		}

		return false;
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
	
	public void SelectLevel01() { changeMenuLevel (0, false); }
	
	public void SelectLevel02() { changeMenuLevel (1, false); }

	public void SelectLevel03() { changeMenuLevel (2, false); }

	public void SelectLevel04() { changeMenuLevel (3, false); }

    public void SelectLevel05() { changeMenuLevel (4, false); }

    public void SelectLevel06() { changeMenuLevel(5, false); }

    public void SelectMainMenu() { changeMenuLevel (0, true); }

	public void changeMenuLevel(int levelIndex, bool back) {
		if (!back) {
			if (!onceLevel) {
				if(GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().transitionFinished) {
					onceLevel = true;
					levelInfo.levelIndex = levelIndices [levelIndex];
					
					OnButtonAccepted ();
					GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().ChangeScenes ("LevelSelectionObject(Clone)", "Scenes/Menu/CharacterSelectionObjectNew", false);
				}
			}
		} else {
			if (!onceLevel) {
				if(GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().transitionFinished) {
					onceLevel = true;
					
					OnButtonDeclined ();
					GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().ChangeScenes ("LevelSelectionObject(Clone)", "Scenes/Menu/MainMenuObject", true);
				}
			}
		}
	}

    private void HandleInput() {

        for (int i = 0; i < maxPlayers; i++) {

            float x = 0.0f;
            
            int runningNumber = i + 1;

            string curHorizontal = "P" + runningNumber + "_Horizontal";
            

            if (Input.GetAxis(curHorizontal) <= -0.2f || Input.GetAxis(curHorizontal) >= 0.2f)
            {
                x = Input.GetAxisRaw(curHorizontal);

            }else if (network.horizontal[i] <= -0.2f || network.horizontal[i] >= 0.2f)
            {
                x = network.horizontal[i];

            }

           

            if (!inputReceived)
            {
                if (x < -0.5f)// && selected != 0)
                {
                    inputReceived = true;
                    moveLeft = true;
                }
                else

                if (x > 0.5f)// && selected != buttons.Length - 1)
                {
                    inputReceived = true;
                    moveRight = true;
                }
            }
        }
    }

    private void ProcessInput() {

        if (moveLeft && !moveRight)
        {

			if (!selectedChange)
			{
				OnButtonSwitched();
				selectedChange = true;
				StartCoroutine("TranslateLeft");
				
			}

        }


        if (!moveLeft && moveRight) {

			if (!selectedChange)
			{
				OnButtonSwitched();
				selectedChange = true;
				StartCoroutine("TranslateRight");
				
			}

        }

    }

	private IEnumerator TranslateRight() {

		float x = 0;
		
		
		StartCoroutine(buttons[selected].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));

		selected += 1;

		if (selected == buttons.Length) {
			selected = 0;
		}

		StartCoroutine(buttons[selected].transform.ScaleTo(new Vector3(1, 1, 1) * 1.0f, 0.35f));

		levelCounter++;

		while (x + Time.deltaTime * 2.6 < 1){
			yield return 0;
			for (int i = 0; i < buttons.Length; i++) {
				buttons [i].transform.Translate (Vector3.left * distance * Time.deltaTime * 2.6f);
			}
			x += Time.deltaTime * 2.6f;
		}
		
		for (int i = 0; i < buttons.Length; i++) {
			buttons [i].transform.Translate (Vector3.left * distance * (1 - x));
		}
		
			buttons[levelTransformBack].transform.Translate (Vector3.right * distance * buttons.Length);

			if (levelTransformBack == buttons.Length - 1) {
				levelTransformBack = 0;
			} else {
				levelTransformBack++;
			}

		selectedChange = false;
		moveRight = false;
	}

	private IEnumerator TranslateLeft() {

		float x = 0;


		levelTransformBack--;
		
		if (levelTransformBack == -1) {
			levelTransformBack = buttons.Length - 1;
		}

		buttons[levelTransformBack].transform.Translate (Vector3.left * distance * (buttons.Length));

		StartCoroutine(buttons[selected].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));

		selected -= 1;

		if (selected == -1) {
			selected = buttons.Length - 1;
		}


		StartCoroutine(buttons[selected].transform.ScaleTo(new Vector3(1, 1, 1) * 1.0f, 0.35f));
		
		while (x + Time.deltaTime * 2.6 < 1){
			yield return 0;
			for (int i = 0; i < buttons.Length; i++) {
				buttons [i].transform.Translate (Vector3.right * distance * Time.deltaTime * 2.6f);
			}
			x += Time.deltaTime * 2.6f;
		}

		for (int i = 0; i < buttons.Length; i++) {
			buttons [i].transform.Translate (Vector3.right * distance * (1 - x));
		}

		selectedChange = false;
		moveLeft = false;
	}



    private void HandleSelection()
    {
        for (int i = 0; i < maxPlayers; i++)
        {           
            int runningNumber = i + 1;

            string submit = "P" + runningNumber + "_Ability";

            if (Input.GetButtonDown(submit) || network.actionButton[i] == 1)
            {
                network.actionButton[i] = 0;
                buttons[selected].GetComponent<Button>().onClick.Invoke();

            }


        }
	}

    void Update()
    {

		if (!onceScale) {
			if (changeButtonPosition ()) {
				StartCoroutine(ScaleLevelImages());
				onceScale = true;
				finishedLoading = true;
			}
		}
	 
        buttons[selected].GetComponent<Button>().Select();

        if (inputReceived)
        {
            inputCooldown -= Time.deltaTime;

            if(inputCooldown <= 0.0f)
            {
                inputCooldown = 0.2f;
                inputReceived = false;


            }
        }

		if (GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().transitionFinished && levelScaled) {
			HandleInput();
			ProcessInput();
			HandleSelection();
		}

		ChangeImages ();

    }

	IEnumerator ScaleLevelImages() {
		
		yield return new WaitForSeconds (0.3f);

		Vector3 originalScale;

		for (int i = 0; i < buttons.Length; i++) {

			if (i == 0) {
				originalScale = new Vector3(1,1,1);
				buttons[0].transform.localScale = Vector3.zero;
				buttons[0].GetComponent<Image>().enabled = true;
				StartCoroutine(buttons[0].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));

				yield return new WaitForSeconds (0.3f);

				
			} else {
				originalScale = buttons[i].transform.localScale;
				buttons[i].transform.localScale = Vector3.zero;
				buttons[i].GetComponent<Image>().enabled = true;
				StartCoroutine(buttons[i].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));
			}
			
		}

		yield return new WaitForSeconds (0.5f);

		levelScaled = true;
		
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

	void ChangeImages() {

		int slotCounter = 0;

		for (int i = 0; i < playerSlot.Length; i++) {
			if (playerSlot[i]) {
				slotCounter++;
			}
		}

		for (int i = 0; i < playerSlotPhone.Length; i++) {
			if (playerSlotPhone[i]) {
				slotCounter++;
			}
		}

		if (oldCounter != slotCounter) {
			if (slotCounter == 0) {
				GameObject.Find ("Player1_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player2_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player3_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player4_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
			} else if (slotCounter == 1) {
				GameObject.Find ("Player1_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player2_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player3_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player4_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
			} else if (slotCounter == 2) {
				GameObject.Find ("Player1_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player2_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player3_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
				GameObject.Find ("Player4_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
			} else if (slotCounter == 3) {
				GameObject.Find ("Player1_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player2_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player3_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player4_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_not_joined_button 1");
			} else if (slotCounter == 4) {
				GameObject.Find ("Player1_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player2_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player3_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
				GameObject.Find ("Player4_Status").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/CharacterSelectionMenu/player_joined_button 1");
			}
		}


		oldCounter = slotCounter;

	}

}
