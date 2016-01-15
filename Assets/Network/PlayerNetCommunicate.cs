using UnityEngine;
using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

using System.Net.NetworkInformation;
using System.Collections;

using UnityEngine.UI;

public class PlayerNetCommunicate : MonoBehaviour
{

	private PlayerManager playerManager;
    [SerializeField]
    private NewCharacterSelectionScript characterMenu;
    private MainMenu mainMenu;
    private LevelSelection levelSelection;

	public UdpClient[] udpListener;
	public UdpClient socket;
	public IPEndPoint[] IP;

	public float[] horizontal; //left
	public float[] vertical; //left
	public float[] horizontalRotation; //right
	public float[] verticalRotation; //right
	
	public int[] actionButton;
	public int[] interaction;
	public int[] stickSize;
	public String gameName = "";
	public String[] deviceID;

	public Boolean created = true;
	public Boolean[] taken;

	int PLAYER = 0;
	int PLAYER_ACTIVE = 0;
	int PORT = 4443;

	public Boolean debugLogs = false;
	
	Text gameNameText;

	List<IPEndPoint> devices;

	IPEndPoint ipSender;

	
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
	
	void Start () {

		ipSender = new IPEndPoint(IPAddress.Parse(LocalIPAddress()), 4441);

        playerManager = GameObject.FindObjectOfType<PlayerManager>();      
        characterMenu = GameObject.FindObjectOfType<NewCharacterSelectionScript>();
        mainMenu = GameObject.FindObjectOfType<MainMenu>();
        levelSelection = GameObject.FindObjectOfType<LevelSelection>();
        udpListener = new UdpClient[4];
		IP = new IPEndPoint[4];


		
		horizontal = new float[4];
		vertical = new float[4];
	
		horizontalRotation = new float[4];
		verticalRotation = new float[4];

		interaction = new int[4];
		actionButton = new int[4];
		stickSize = new int[4];
		taken = new bool[4];

		deviceID = new string[4];

		devices = new List<IPEndPoint>();

		CreateGameName ();

		for (int i = 0; i < 4; i++) {

			horizontal[i] = 0.0f;
			vertical[i] = 0.0f;
			
			horizontalRotation[i] = 0.0f;
			verticalRotation[i] = 0.0f;

			actionButton [i] = 0;
			interaction[i] = 0;
			stickSize [i] = 0;
			taken[i] = false;
			
		}

		try {
			socket = new UdpClient(4441);
		} catch (Exception e) {
			Debug.Log (e.Message);
		}

		BroadcastIP ();

        int counter = 0;

        while (counter < 4)
        {

            if (created)
            {

                created = false;

                udpListener[counter] = new UdpClient(PORT);
                IP[counter] = new IPEndPoint(IPAddress.Parse(LocalIPAddress()), PORT);

                PLAYER = counter;

                counter++;
                PORT++;

                InitializeListenerUdp();

            }

        }

		GameObject textObject = GameObject.Find ("GameName");
		
		if (textObject != null) {
			gameNameText = textObject.GetComponent<Text>();
			gameNameText.text = gameName + " GAME";
		}

	}

	public void CreateGameName() {

		String[] names = {"FUN", "FANCY", "UGLY", "RED", "BETTER", "EASY",
			"ODD", "WRONG", "GRUMPY", "IMPORTANT", "HILARIOUS", "DEEP",
			"DEADLY", "GREAT", "HUGE", "SEXY", "CRAZY", "DUMB", "BORING",
			"CRAP", "DERP", "PANTS", "FURIOUS", "FLUFFY", "YUMMY", "ROYAL",
			"DECENT", "CAT", "SCARY", "FAT", "BUGGY", "MOON", "NEW", "SAVE", 
			"RANDOM", "FUNKY", "HUMID", "FLIRT", "GLOBAL", "FRIED", "LETHAL",
			"QUICK", "CUTE", "BLOODY", "BOARD", "WEIRD", "JUICY", "ARCADE", 
			"SAD", "BORING", "BOLD", "FRESH", "GROOVY", "PAIN", "END", "GRAND", 
			"GUN", "POLY", "WAR", "BRUTAL", "GROSS", "NUDE", "GAY", "FLOWER", 
			"FALSE", "SWEET", "PUBLIC", "SPEED", "SOUP", "TOTAL", "POETIC", 
			"SLEEP", "TRAP", "HONEST", "STRANGE", "BELLY", "GRIND", "HOLY", 
			"ZOO", "FARMER", "CANDY", "RPG", "MMO", "FILTHY", "TRAGIC", "FIRST",
			"BAD", "HORSE", "HAND", "FOOT", "EYE", "I", "FRUITY", "DOG", "CAGE",
			"PAST", "FUTURISTIC", "BANANA", "HAWAII", "PLAY", "BABY"};

		System.Random rnd = new System.Random();
		int location = rnd.Next(names.Length - 1);

		gameName = names [location];

	}

	void Update () {
		characterMenu = GameObject.FindObjectOfType<NewCharacterSelectionScript>();
		mainMenu = GameObject.FindObjectOfType<MainMenu>();
		levelSelection = GameObject.FindObjectOfType<LevelSelection>();
	}

    void OnLevelWasLoaded(int level)
    {

			playerManager = GameObject.FindObjectOfType<PlayerManager>();      
			int counter = 0;
			created = true;
			
			while (counter < 4)
			{
				if (created)
				{
					created = false;
					PLAYER = counter;
					InitializeListenerUdp();
					counter++;
				}
				
			}

			BroadcastIP();

		GameObject textObject = GameObject.Find ("GameName");
		
		if (textObject != null) {
			gameNameText = textObject.GetComponent<Text>();
			gameNameText.text = gameName + " GAME";
		}
        

    }

	void InitializeListenerUdp () {

		UnityThreadHelper.CreateThread (() => {

			int slot = PLAYER;

			if (debugLogs) {
				Debug.Log ("Creating listener for player " + slot + " on port: " + IP [slot].Port);
			}

			created = true;

			while (true) {

				byte[] answerByte = udpListener [slot].Receive (ref IP [slot]);
				String data = Encoding.ASCII.GetString (answerByte, 0, answerByte.Length);
				String caseIdentification = data.Substring (0, 1);

				switch (caseIdentification) {
		
				case "0":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleIdentification (data.Substring (1), slot));
					break;

				case "1":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleLeft (data.Substring (1), slot));
					break;

				case "2":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleRight (data.Substring (1), slot));
					break;

				case "3":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleActionButton(data.Substring (1), slot));
					break;

				case "4":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleInteraction(data.Substring (1), slot));
					break;

				case "5":
					UnityThreadHelper.Dispatcher.Dispatch(() => handleStickSize(data.Substring (1), slot));
					break;

				}

			}

		});

	}

	void handleIdentification (String data, int slot) {

		String actionCase = "";
		String DEVICE_ID = "";

		if (data.Length > 3) {
			actionCase = data.Substring (0, 3);
			DEVICE_ID = data.Substring (3);
		} else {
			actionCase = data;
		}

		if (debugLogs) {
			Debug.Log(actionCase);
			Debug.Log(DEVICE_ID);
		}

		switch (actionCase) {

		case "001":

			String send;
			Boolean playerAlradyInGame = false;

			for (int i = 0; i < 4; i++) {
				if (DEVICE_ID.Equals(deviceID[i])) {
					playerAlradyInGame = true;
					break;
				}
			}

			if (playerAlradyInGame && DEVICE_ID.Equals(deviceID[slot])) {
				taken[slot] = true;
				send = "0001";
			}

			else if (taken[slot] == false && !playerAlradyInGame) {
				send = "0001";
                if (playerManager != null)
                {
                    //UnityThreadHelper.Dispatcher.Dispatch(() => playerManager.HandlePhonePlayerJoin(slot));
                }
                if (characterMenu != null)
                {
                    UnityThreadHelper.Dispatcher.Dispatch(() => characterMenu.HandlePhonePlayerJoin(slot));
                }
                if (mainMenu != null)
                {
                    UnityThreadHelper.Dispatcher.Dispatch(() => mainMenu.HandlePhonePlayerJoin(slot));
                }
                if(levelSelection != null)
                {
                    UnityThreadHelper.Dispatcher.Dispatch(() => levelSelection.HandlePhonePlayerJoin(slot));
                }
				
				deviceID[slot] = DEVICE_ID;
				PLAYER_ACTIVE++;
				taken[slot] = true;
			}

			else {
				send = "0002" ;
			}

			byte[] sendByte = Encoding.ASCII.GetBytes (send);
			udpListener [slot].Send (sendByte, sendByte.Length, IP [slot]);

			break;

		case "002":

            if (characterMenu != null)
            {
                UnityThreadHelper.Dispatcher.Dispatch(() => characterMenu.PlayerPhoneLeave(slot));

            }    
			taken[slot] = false;
			break;

		case "003":

			sendData("0005", slot);
			break;

		}

	}

	void handleLeft (String data, int slot) {

		if (((int)stickSize [slot]) != 0) {
			horizontal [slot] = ((float.Parse (data.Substring(0,4))) / stickSize[slot] * -1.0f);
			vertical [slot] = ((float.Parse (data.Substring(4,4))) / stickSize[slot] * -1.0f);
		}

		if (float.IsNaN (horizontal [slot])) {
			horizontal [slot] = 0.0f;
		}

		if (float.IsNaN (vertical [slot])) {
			vertical [slot] = 0.0f;
		}
	}

	void handleRight (String data, int slot) {
		if (((int)stickSize [slot]) != 0) {
			horizontalRotation [slot] = ((float.Parse (data.Substring(0,4))) / stickSize[slot] * -1.0f);
			verticalRotation [slot] = ((float.Parse (data.Substring(4,4))) / stickSize[slot] * -1.0f);
		}

		if (float.IsNaN (horizontalRotation [slot])) {
			horizontalRotation [slot] = 0.0f;
		}
		
		if (float.IsNaN (verticalRotation [slot])) {
			verticalRotation [slot] = 0.0f;
		}
	}
	
	void handleActionButton(String data, int slot) {
		actionButton [slot] = int.Parse (data);
		StartCoroutine (SetActionButtonBack (slot));
	}

	IEnumerator SetActionButtonBack (int slot) {
		yield return new WaitForSeconds (0.2f);
		actionButton [slot] = 0;
	}

	void handleInteraction(String data, int slot) {
		interaction [slot] = int.Parse (data);
	}

	void handleStickSize(String data, int slot) {
		stickSize [slot] = int.Parse (data);
	}

	public void sendData(String str, int slot) {
		IPEndPoint endpoint = new IPEndPoint (IP[slot].Address, 4442);
		byte[] send = Encoding.ASCII.GetBytes (str);
		udpListener [slot].Send(send, send.Length, endpoint);	
	}

	void BroadcastIP () {
		
		UnityThreadHelper.CreateThread (() => {

			String data = "";

			try {
				byte[] answerByte = socket.Receive (ref ipSender);
				data = Encoding.ASCII.GetString (answerByte, 0, answerByte.Length);
			} catch (Exception e) { Debug.Log(e); }
					
			if (debugLogs) {
				Debug.Log(data);
			}
					
			if (data.Equals("0000")) {
				string str = "0000" + gameName;
				byte[] sendBytes = Encoding.ASCII.GetBytes(str);
				IPEndPoint sendIP = new IPEndPoint(ipSender.Address, 4442);
				devices.Add(sendIP);
				socket.Send(sendBytes, sendBytes.Length, sendIP);
			}

			UnityThreadHelper.Dispatcher.Dispatch(() => BroadcastIP()); 

		});
	}

	void EndBroadcastIP() {

		UdpClient close = null;

		try {
			close = new UdpClient();
		} catch (Exception e) {
			Debug.Log (e.Message);
		}

		for (int i = 0; i < devices.Count; i++) {
			string str = "0004";
			byte[] sendBytes = Encoding.ASCII.GetBytes(str);
			close.Send(sendBytes, sendBytes.Length, devices[i]);
		}

		try {
			close.Close ();
		} catch (Exception e) {
			Debug.Log (e.Message);
			Debug.Log ("Could not close close socket!");
		}



	}

    void OnApplicationQuit()
    {
		if (debugLogs) {
			Debug.Log ("ondestroy");
		}

		for (int i = 0; i < PLAYER_ACTIVE; i++) {					
			sendData("0003", i);
		}

		EndBroadcastIP ();

        try
        {
            for (int i = 0; i < PLAYER + 1; i++) {
                udpListener[i].Close();
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("Could not close listener!");
        }

		try {
			socket.Close ();
		} catch (Exception e) {
			Debug.Log (e.Message);
			Debug.Log ("Could not close socket!");
		}

	}
	
	public string LocalIPAddress ()
	{
		IPHostEntry host;
		string localIP = "";
		host = Dns.GetHostEntry (Dns.GetHostName ());
		foreach (IPAddress ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				localIP = ip.ToString ();
				break;
			}
		}
		return localIP;
	}
}