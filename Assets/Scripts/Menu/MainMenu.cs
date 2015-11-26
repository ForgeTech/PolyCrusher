using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Delegate event handler for buttons switches.
/// </summary>
public delegate void ButtonSwitchedEvent();

/// <summary>
/// Delegate event handler for button accepting.
/// </summary>
public delegate void ButtonAcceptedEvent();

/// Delegate event handler for button declining.
/// </summary>
public delegate void ButtonDeclinedEvent();

/// <summary>
/// Menu script for the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{

    public GameObject[] buttons;
    public int selected;

    private bool moveLeft = false;
    private bool moveRight = false;

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
	
    /// <summary>
    /// Loads the level selection menu.
    /// </summary>
    public void CrushPolys()
    {
		if (!once) {
			if(GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().transitionFinished) {
				once = true;

				OnButtonAccepted();
				GameObject.Find ("_StartMenu").GetComponent<StartMenu> ().ChangeScenes ("MainMenuObject(Clone)", "Scenes/Menu/LevelSelectionObject", false);
			}
		}

		//StartCoroutine(WaitUntilAcceptSoundFinished(1));
    }

    void Awake()
    {
        GameObject[] music = GameObject.FindGameObjectsWithTag("Music");

        if (music != null)
        {
            if (music.Length > 1)
            {
                Destroy(music[1]);
            }

            DontDestroyOnLoad(music[0]);
        }
    }
	
    void Start()
    {
		buttons [0].GetComponent<Button> ().Select ();
       // buttons[0].GetComponentInChildren<Text>().color = Color.yellow;

		// Register switch sound.
		ButtonSwitchEvent += PlaySwitchSound;
		ButtonAcceptEvent += PlayAcceptSound;
		ButtonDeclineEvent += PlayDeclineSound;

		inputCooldown = inputCooldownTime;

		levelInfo = GameObject.FindObjectOfType<LevelStartInformation> ();
		network = GameObject.FindObjectOfType<PlayerNetCommunicate> ();

		playerSlotPhone = new bool[4];

		playerCount = Input.GetJoystickNames ().Length;
		currentPlayerCount = playerCount;

		Debug.Log (playerCount);

		for (int i = 0; i < playerCount; i++) {
           
			playerSlot [i] = true;
			levelInfo.playerSlotTaken [i] = true;
         
		}

		for (int i = 0; i < levelInfo.phonePlayerSlotTaken.Length; i++) {
			if (levelInfo.phonePlayerSlotTaken [i] && playerCount < 4) {
				playerSlotPhone [i] = true;
				//levelInfo.phonePlayerSlotTaken[i] = true;
				currentPlayerCount++;

			}
		}

		GameObject.Find ("GameName").GetComponent<Text> ().text = network.gameName + " GAME";

		once = false;

		StartCoroutine ("ScalePlayerImages");
		StartCoroutine ("TransformBetabanner");

	}

	IEnumerator ScalePlayerImages() {

		GameObject[] buttonsNew = new GameObject[3];
        
		
		buttonsNew[0] = GameObject.Find("Crush_Poly_Text");
		buttonsNew[1] = GameObject.Find("Game_Stats_Text");
		buttonsNew[2] = GameObject.Find("Leave_Game_Text");

        Text[] texts = GameObject.FindObjectsOfType<Text>();

		for (int i = 0; i < buttons.Length; i++) {
			buttons[i].GetComponent<Image>().enabled = false;
		}
		
        for(int i = 0; i < texts.Length; i++)
        {
            texts[i].enabled = false;
        }


		yield return new WaitForSeconds (0.3f);
		
		for (int i = 0; i < buttonsNew.Length; i++) {
			Vector3 originalScale = buttonsNew[i].transform.localScale;
			buttonsNew[i].transform.localScale = Vector3.zero;
			buttonsNew[i].GetComponent<Image>().enabled = true;
			StartCoroutine(buttonsNew[i].transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));
		}
        for (int i = 0; i < texts.Length; i++)
        {
            if(texts[i]!=null)
            texts[i].enabled = true;
        }

    }

	IEnumerator TransformBetabanner(){

		GameObject banner1 = GameObject.Find ("Image");
		GameObject banner2 = GameObject.Find ("Image 1");
	
		x = 0;

		while (x < 3200) {
			yield return 0;
			banner1.transform.localPosition = new Vector2 (banner1.transform.localPosition.x - 100 * Time.deltaTime, banner1.transform.localPosition.y + Time.deltaTime * 1.7f);
			banner2.transform.localPosition = new Vector2 (banner2.transform.localPosition.x + 100 * Time.deltaTime, banner2.transform.localPosition.y + Time.deltaTime * 7.3f);
			x += 100 * Time.deltaTime;
		}

		x = 0;
		
		while (x < 3200) {
			yield return 0;
			banner1.transform.localPosition = new Vector2 (banner1.transform.localPosition.x + 100 * Time.deltaTime, banner1.transform.localPosition.y - Time.deltaTime * 1.7f);
			banner2.transform.localPosition = new Vector2 (banner2.transform.localPosition.x - 100 * Time.deltaTime, banner2.transform.localPosition.y - Time.deltaTime * 7.3f);
			x += 100 * Time.deltaTime;
		}

		StartCoroutine ("TransformBetabanner");

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

        buttons[selected].GetComponent<Button>().Select();
        

        for(int i = 0; i < buttons.Length; i++)
        {
            Text[] texts = buttons[i].GetComponentsInChildren<Text>();
            if (i!= selected)
            {
                texts[1].color = Color.red;
            }
            else
            {
                texts[1].color = Color.yellow;
            }
           
           

        }
       // buttons[selected].GetComponentInChildren<Text>().color = Color.yellow;

        if (inputReceived)
        {
            inputCooldown -= Time.deltaTime;

            if (inputCooldown <= 0.0f)
            {
                inputCooldown = inputCooldownTime;
				inputReceived = false;

            }
        }

        HandleInput();
        ProcessInput();
        HandleSelection();

		ChangeImages ();

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
                    playerSlot[i] = true ;
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


            if (Input.GetAxis(curHorizontal) <= -0.2f || Input.GetAxis(curHorizontal) >= 0.2f )
            {
                x = Input.GetAxisRaw(curHorizontal);

            } else if( network.horizontal[i] <= -0.2f || network.horizontal[i] >= 0.2f)
            {

                x = network.horizontal[i];
            }



            if (!inputReceived)
            {
                if (x < -0.5f && selected != 0)
                {
                    inputReceived = true;
                    moveLeft = true;
                }
                else

                if (x > 0.5f && selected != buttons.Length - 1)
                {
                    inputReceived = true;
                    moveRight = true;
                }

            }



        }



    }

    private void ProcessInput()
    {
        if (moveLeft && !moveRight)
        {

            if (!selectedChange)
            {
                selectedChange = true;
                selected -= 1;
                OnButtonSwitched();
            }

            animationTime += Time.deltaTime;

            if (animationTime > inputCooldownTime)
            {
                selectedChange = false;
                animationTime = 0.0f;
                moveLeft = false;
            }
        }


        if (!moveLeft && moveRight)
        {
            if (!selectedChange)
            {
                selectedChange = true;
                selected += 1;
                OnButtonSwitched();
            }

            animationTime += Time.deltaTime;

            if (animationTime > inputCooldownTime)
            {
                animationTime = 0.0f;
                selectedChange = false;
                moveRight = false;
            }
        }
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
                buttons[selected].GetComponent<Button>().onClick.Invoke();
            }


        }

    }

    public void Stats()
    {
        //TODO - Load right level:
        //StartCoroutine(WaitUntilSoundFinished(index));

        if (!once)
        {
            if (GameObject.Find("_StartMenu").GetComponent<StartMenu>().transitionFinished)
            {
                once = true;

                OnButtonAccepted();
                GameObject.Find("_StartMenu").GetComponent<StartMenu>().ChangeScenes("MainMenuObject(Clone)", "Scenes/Menu/OptionMenuObject", false);
            }
        }
    }


    /// <summary>
    /// Exits the game.
    /// </summary>
    public void Exit() {
		if (!once) {
			once = true;
			OnButtonDeclined();
			Application.Quit();
		}
    }

    /// <summary>
    /// Plays the switch sound.
    /// </summary>
    protected void PlaySwitchSound()
    {
        if(switchSound != null)
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