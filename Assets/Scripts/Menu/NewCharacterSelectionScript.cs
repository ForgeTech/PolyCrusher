using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NewCharacterSelectionScript : MonoBehaviour {


    public GameObject[] playerImages;
    public float inputCoolDownTime;
    private Vector3 scale;

    private float heightDistance;


    private PlayerNetCommunicate network;
    private LevelStartInformation levelInfo;
    private Canvas canvas;

    private GameObject[,] selectionImages; 
    public int[] selectedCharacters;
    private int[] hoveredCharacters;
    [SerializeField]
    private int[] playerCursorSlot;


    private int[] transformBack;
    private int[] currentControllerInput;
    private int maxPlayers;
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




    // Use this for initialization
    void Start () {

        maxPlayers = 4;
        playerCursorSlot = new int[] {-1,-1,-1,-1 };
        middlePos = new Vector2[4];
        for(int i = 0; i < middlePos.Length; i++)
        {
            middlePos[i].Set(Screen.width / 4.5f + (Screen.width / 5 * i), Screen.height/2*2.8f );

        }

        selectedCharacters = new int[] {-1, -1, -1, -1 };
        hoveredCharacters = new int[] { 0, 0, 0, 0 };
        currentControllerInput = new int[4];
        inputReceived = new bool[4];
        inputCooldown = new float[] {inputCoolDownTime, inputCoolDownTime, inputCoolDownTime, inputCoolDownTime };
        selectedChange = new bool[4];

        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
        network = GameObject.FindObjectOfType<PlayerNetCommunicate>();
        canvas = GameObject.FindObjectOfType<Canvas>();

        playerSlotPhone = new bool[4];

        playerCount = Input.GetJoystickNames().Length;
        currentPlayerCount = playerCount;

        scale = new Vector3();
        scale.Set(0.0f, 0.0f,1.0f);

        Debug.Log(playerCount);
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
                    playerCursorSlot[playerCount + i] = 5 + i;
                    playerSlotPhone[i] = true;
                    currentPlayerCount++;

                }
            }
        }

        heightDistance = Screen.height / 3;

        selectionImages = new GameObject[4,playerImages.Length];
        for(int i = 0; i < selectionImages.GetLength(0); i++)
        {
            for(int j = 0; j < selectionImages.GetLength(1); j++)
            {
                selectionImages[i, j] = Instantiate(playerImages[j], new Vector2(middlePos[i].x, middlePos[i].y+(heightDistance*j)), Quaternion.identity) as GameObject;
                selectionImages[i, j].transform.SetParent(canvas.transform);
                selectionImages[i, j].transform.localScale = scale;


            }


        }

        

        transformBack = new int[] { playerImages.Length - 1, playerImages.Length - 1, playerImages.Length - 1, playerImages.Length - 1 };


        

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

    // Update is called once per frame
    void Update () {



        if (!once)
        {
            if (changeButtonPosition())
            {
                Debug.Log(-1%3);
                ScaleImages();
                once = true;
            }
        }








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

        UpdatePlayerStatus();
        HandleInput();
        ProcessInput();
        HandleSelection();
        UpdateImageDisplay();

        

    }

 
    private void ScaleImages()
    {
        for (int i = 0; i < selectionImages.GetLength(0); i++)
        {           
            int jMinus = 0;
            int jPlus = 0;

            if(hoveredCharacters[i] == 0)
            {
                jMinus = playerImages.Length - 1;
                jPlus = 1;
            }else if(hoveredCharacters[i] == playerImages.Length - 1)
            {
                jMinus = playerImages.Length - 2;
                jPlus = 0;
            }
            else
            {
                jMinus = hoveredCharacters[i] - 1;
                jPlus = hoveredCharacters[i] + 1;
            }
                              
            StartCoroutine(selectionImages[i, hoveredCharacters[i]].transform.ScaleTo(new Vector3(1, 1, 1) * 0.6f, 0.35f));
            //StartCoroutine(selectionImages[i, jMinus].transform.ScaleTo(new Vector3(1, 1, 1) * 0.35f, 0.35f));
            //StartCoroutine(selectionImages[i, jPlus].transform.ScaleTo(new Vector3(1, 1, 1) * 0.35f, 0.35f));         
        }
    }



    private void UpdateImageDisplay()
    {
        for(int i = 0; i < maxPlayers; i++)
        {
            for(int j = 0; j < selectedCharacters.Length; j++)
            {
                if(i!= j && hoveredCharacters[i] == selectedCharacters[j])
                {
                    StartCoroutine(MoveUpwards(i));
                }
            }
        }
    }



    private void HandleSelection()
    {
        for(int i = 0; i < selectionImages.GetLength(0); i++)
        {
            int runningNumber = i + 1;

            string submit = "P" + runningNumber + "_Ability";

            if (Input.GetButtonDown(submit) || network.actionButton[i] == 1)
            {
                network.actionButton[i] = 0;
                selectedCharacters[i] = hoveredCharacters[i];

            }
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
            for (int i = 0; i < playerSlot.Length; i++)
            {
                if (!controllerAdded && playerSlot[i] == false)
                {
                    playerSlot[i] = true;
                    if (levelInfo != null)
                    {
                        levelInfo.playerSlotTaken[i] = true;
                    }                    
                    controllerAdded = true;
                }
            }
        }

    }



    private void ProcessInput()
    {
        for(int i = 0; i < playerCursorSlot.Length; i++)
        {
            if(playerCursorSlot[i] != -1 && currentControllerInput[i] != -1)
            {
                Debug.Log("Player " + (i + 1) + " pressed " + currentControllerInput[i]);

                if (!selectedChange[i])
                {
                    
                    if (currentControllerInput[i] == 1)
                    {
                        Debug.Log("1!!!!!");  
                        StartCoroutine(MoveUpwards(i));
                        selectedChange[i] = true;
                    }

                    if (currentControllerInput[i] == 3)
                    {
                        Debug.Log("3!!!!!");
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
            if (playerCursorSlot[i] < 5 && playerCursorSlot[i] > -1)
            {               
                int runningNumber = playerCursorSlot[i] + 1;

                string curHorizontal = "P" + runningNumber + "_Horizontal";
                string curVertical = "P" + runningNumber + "_Vertical";

                x = Input.GetAxisRaw(curHorizontal);
                y = Input.GetAxisRaw(curVertical);
                
            }

            else if (playerCursorSlot[i] > 4)
            {              
                x = network.horizontal[playerCursorSlot[i] % 5];
                y = network.vertical[playerCursorSlot[i] % 5];
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





    private void UpdatePlayerStatusOld()
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

                    playerCursorSlot[i] = newPhoneCount + 4;                   
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
        Debug.Log("move upwards");
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


}
