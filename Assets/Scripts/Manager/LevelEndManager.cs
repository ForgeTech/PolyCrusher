using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine.UI;

/// <summary>
/// Level exit delegate.
/// </summary>
public delegate void LevelExitDelegate();

public class LevelEndManager : MonoBehaviour {

	//Camera cam;
	GameObject camObject;
	GameManager gameManager;

	Image im;

	Text t1;
	Text t2;
	Text t3;
	Text t4;
	Text t5;
    Text[] t6;

    //Vector3 originalScaleYolo;
    Vector3 originalScale;
	Vector3 originalScale1;
	Vector3 originalScale4;
	Vector3 originalScale5;
	Vector3 originalScale6;
    Vector3[] originalScaleTexts;


	GameObject nameField;

    UnityEngine.UI.Image redOverlay;
    UnityEngine.UI.Text yoloTime;

	Boolean dead = false;

	float power;
	float time;
	public float seconds = 0.5f;
	
	PlayerNetCommunicate network;

	public Boolean endScreenIsShown = false;

	public String playerGameName;

	bool once;

    

	public static event LevelExitDelegate levelExitEvent;
	
	void Awake ()
    {
		PlayerManager.AllPlayersDeadEventHandler += ShowEndScreen;
		DataCollector.RankReceived += GetOnlineRank;
	}

	void Start ()
    {
		camObject = GameObject.FindGameObjectWithTag("MainCamera");       

		if (camObject != null) {

			//cam = camObject.GetComponent<Camera>();

			UnityStandardAssets.ImageEffects.ColorCorrectionCurves ccc = camObject.GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves> ();

			if (ccc != null) {
				ccc.enabled = false;
			}

		}

		nameField = GameObject.Find("NameField");

		if (nameField != null) {
			nameField.SetActive (false); 
		}

		network = GameObject.FindObjectOfType<PlayerNetCommunicate> ();
		dead = false;
		once = false;

        redOverlay = GameObject.Find("RedOverlay").GetComponent<Image>();
        yoloTime = redOverlay.GetComponentInChildren<Text>();
	}

	void ShowEndScreen()
    {
        /*//========Test=========
        if (playerManagerReference != null)
        {
            string[] playerNames = playerManagerReference.PlayerNamesInGameSession;
            BasePlayer[] players = playerManagerReference.PlayersInGameSession;

            Debug.Log("PlayerGameSessionCount: " + playerManagerReference.PlayerCountInGameSession);

            for (int i = 0; i < playerManagerReference.PlayerCountInGameSession; i++)
            {
                Debug.Log("PlayerNames: " + playerNames[i]);
                Debug.Log("PlayerReferences: " + players[i].PlayerName + ", " + players[i].transform.position);
            }
            
        }
        //=====================*/

        if (camObject != null && nameField != null)
        {
            if(GameManager.GameManagerInstance.CurrentGameMode == GameMode.YOLOMode)
            {
                TimeUtil playtime = PlayerManager.PlayTime;

                redOverlay.enabled = true;
                yoloTime.enabled = true;
                yoloTime.text = string.Format("{0:00}:{1:00}:{2:00}", playtime.Minute, playtime.Second, playtime.Milliseconds);
            }

			im = GameObject.Find ("InnerCircle").GetComponent<Image> ();
				
			t1 = GameObject.Find ("InWaveNumber").GetComponent<Text> ();
			t2 = GameObject.Find ("RankingNumber").GetComponent<Text> ();
			t3 = GameObject.Find ("CrushedNumber").GetComponent<Text> ();
			t4 = GameObject.Find ("PlayerNumber").GetComponent<Text> ();

            t6 = im.GetComponentsInChildren<Text>();
				
			//GameObject.Find ("JoinText").GetComponent<Text> ().enabled = false;
			//GameObject.Find ("GameName").GetComponent<Text> ().enabled = false;
            GameObject waveRessourceBar = GameObject.Find("WaveRessourceBar");
            if (waveRessourceBar != null)
                waveRessourceBar.GetComponent<Image>().enabled = false;

            GameObject waveRessourceGreen = GameObject.Find("WaveRessourceBarGreenBack");
            if(waveRessourceGreen != null)
                waveRessourceGreen.GetComponent<Image>().enabled = false;

            GameObject waveNumberPermanent = GameObject.Find("WaveNumberPermanent");
            if(waveNumberPermanent != null)
                waveNumberPermanent.GetComponent<Text>().enabled = false;


            t1.enabled = true;
			t2.enabled = true;
			t3.enabled = true;
			t4.enabled = true;	
			im.enabled = true;

            originalScaleTexts = new Vector3[t6.Length];

            for (int i = 0; i < t6.Length; i++)
            {
                if(t6[i].name != "InWaveNumber" && t6[i].name != "Text" &&t6[i].name != "Loading")
                {
                    if (t6[i].name == "InWave")
                    {
                        if (GameManager.GameManagerInstance.CurrentGameMode != GameMode.YOLOMode)
                        {
                            originalScaleTexts[i] = t6[i].transform.localScale;
                            t6[i].transform.localScale = Vector3.zero;
                            t6[i].enabled = true;
                        }
                    }
                    else
                    {
                        originalScaleTexts[i] = t6[i].transform.localScale;
                        t6[i].transform.localScale = Vector3.zero;
                        t6[i].enabled = true;
                    }

                   
                }
               
            }



            //originalScaleYolo = yoloTime.transform.localScale;
            yoloTime.transform.localScale = Vector3.zero;

			originalScale = im.transform.localScale;
			im.transform.localScale = Vector3.zero;

			originalScale1 = t1.transform.localScale;
			t1.transform.localScale = Vector3.zero;

           

			originalScale4 = t2.transform.localScale;
			t2.transform.localScale = Vector3.zero;

            Debug.Log("oriscale before: " + originalScale4);

            originalScale5 = t3.transform.localScale;
			t3.transform.localScale = Vector3.zero;

			originalScale6 = t4.transform.localScale;
			t4.transform.localScale = Vector3.zero;

			t1.text = GameManager.gameManagerInstance.Wave + "!";

			nameField.SetActive (true);
            nameField.GetComponent<InputField>().Select();
            nameField.GetComponent<InputField>().ActivateInputField();
			StartCoroutine(WaitForScaleFinish ());
				
			camObject.GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves> ().enabled = true;
				
			dead = true;
			time = 0.0f;

			StartCoroutine(DataCollector.instance.DownlaodHighscoreRank());
							
		}

	}

	IEnumerator WaitForScaleFinish() {
		GameObject.Find ("InnerCircle").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/LevelFinished/innerCircle_withoutText");
       
        for (int i = 0; i < t6.Length; i++)
        {
            if (t6[i].name != "Text" && t6[i].name != "InWaveNumber" && t6[i].name != "Loading")
            {
                StartCoroutine(t6[i].transform.ScaleTo(originalScaleTexts[i], 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));
            }
            
        }
		StartCoroutine(im.transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.grow.Evaluate));
		yield return new WaitForSeconds (1.0f);
		endScreenIsShown = true;
	}

	void Update() {

		if (dead) {

			power = Mathf.Lerp (1, 0, time);

			if (time <= 1) {
				time += Time.deltaTime/seconds;
			}

			camObject.GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves> ().saturation = power; 
		}

		if (endScreenIsShown) {

			Boolean startMenu = false;

			if (Input.GetButtonDown("P1_Ability") || Input.GetButtonDown("P2_Ability") || Input.GetButtonDown("P3_Ability") || Input.GetButtonDown("P4_Ability")) {
				startMenu = true;
			}
		
			//for (int i = 0; i < 4; i++) {
			//	if (network.actionButton[i] == 1) {
			//		startMenu = true;
			//		break;
			//	}
			//}

			//for (int i = 0; i < 4; i++) {
			//	network.actionButton[i] = 0;
			//}

			if (startMenu) {

				endScreenIsShown = false;

				if (!once) {
					once = true;
					StartMainMenu();
				}
			}
		}
	}

	protected IEnumerator ShowNumbers()
	{
        yield return new WaitForSeconds(1.0f);

        

        if (GameManager.GameManagerInstance.CurrentGameMode != GameMode.YOLOMode)
        {
            //yield return new WaitForSeconds(1.0f);

            Debug.Log("non yolo triggered:  "+originalScale1);
            StartCoroutine(t1.transform.ScaleTo(originalScale1, 0.3f, AnimCurveContainer.AnimCurve.shortUpscale.Evaluate));
        }
        else
        {
            StartCoroutine(yoloTime.transform.ScaleTo(originalScale1, 0.3f, AnimCurveContainer.AnimCurve.shortUpscale.Evaluate));
        }

		yield return new WaitForSeconds(0.3f);
		
		StartCoroutine(t2.transform.ScaleTo(originalScale4, 0.3f, AnimCurveContainer.AnimCurve.grow.Evaluate));
		
		yield return new WaitForSeconds(0.3f);
		
		StartCoroutine(t3.transform.ScaleTo(originalScale5, 0.3f, AnimCurveContainer.AnimCurve.grow.Evaluate));
		
		yield return new WaitForSeconds(0.3f);
		
		StartCoroutine(t4.transform.ScaleTo(originalScale6, 0.3f, AnimCurveContainer.AnimCurve.grow.Evaluate));
		
	}

	public void StartMainMenu()
    {
        redOverlay.enabled = false;

        foreach(Text t in t6)
        {
            if(t.name == "Loading")
            {
                Debug.Log("loading set to visible");
                t.enabled = true;
            }
            else
            {
                t.enabled = false;
            }
            
        }


        GameObject.Find ("InnerCircle").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Menu/LevelFinished/loadingScreen_withoutText");



		network.CreateGameName ();

		String enteredName = "";
		GameObject textInput = GameObject.Find ("TextInput");

		if (textInput != null) {
			Text stringTextInput = textInput.GetComponent<Text> ();
			if (stringTextInput != null) {
				enteredName = stringTextInput.text;
			}
		}

		if (!enteredName.Trim ().Equals ("")) {
			playerGameName = enteredName;
		} else {
			playerGameName = GameObject.FindObjectOfType<PlayerNetCommunicate>().gameName;
		}

		t1.enabled = false;
		t2.enabled = false;
		t3.enabled = false;
		t4.enabled = false;

		nameField.SetActive (false);

		StartCoroutine(LoadMain());

	}

	protected IEnumerator LoadMain()
	{
		yield return new WaitForSeconds(1.0f);
		OnLevelExit ();
		levelExitEvent = null;
		Application.LoadLevel (0);
	}

	protected void OnLevelExit() {
		if (levelExitEvent != null) {
			levelExitEvent();
		}
	}

	void GetOnlineRank(int rank) {

			if (rank == 1) {
				t2.text = rank + "st";
			} else if (rank == 2) {
				t2.text = rank + "nd";
			} else if (rank == 3) {
				t2.text = rank + "rd";
			} else {
				t2.text = rank + "th";
			}
			
			t3.text = DataCollector.instance.playerWithLeastDeathtime().ToUpper();
			t4.text = DataCollector.instance.playerWithMostKills().ToUpper();
			
			StartCoroutine("ShowNumbers");
		
	}

}
