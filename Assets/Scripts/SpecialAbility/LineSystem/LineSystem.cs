using UnityEngine;
using System.Collections;

 public enum LineStatus
{
    normal, healAnimation, heal, normalAnimation
}

public class LineSystem : MonoBehaviour {

    #region variables
    public float lineStartOffset;
    public float healAmount;
    public float energyHealAmount;
    public float normalLineWidth;
    public float healLineWidth;
    public float healDistance;
    public float lerpSpeed;
    public float healDistancePadding;
    public float polyDistance;

    [Header("Set the active time for the cutting lines Power-Up")]
    public int powerUpTime = 12;

    public Color[] colors;

    [SerializeField]
    private Material lineShaderMaterial;
    public GameObject health;
    public GameObject energy;
    public GameObject laserParticles;


    private ParticleSystem[] energyParticles;
    private ParticleSystem[] healthParticles;


    private bool isChangingHeal;
    private bool isChangingNormal;



    private GameObject[] players;
    private GameObject[] healingparticles;


   
    private bool[] isChangingColor;
    private bool[] isChangingAmplitude;

    private float[] incremenetTimers;  

    private bool[] playerMoved;
    private bool[] playerHealing;
    private bool[] playerWasHealed;
    private int[] playersInReach;

    private bool[,] nearbyPlayers;


    private BasePlayer[] playerScripts;
    private LineShaderUtility[] lineShaderUtilities;
    private int[] linesNeeded;
    private int[] firstVertex;
    private int[] secondVertex;
   
    private bool[] energyFillUp;

    private bool sabreAnimationFinished;

    private int[] activeColor;
    private bool[] activeHealSineLine;
    private int lineRendererLength;
    
    [SerializeField]
    private bool activateCutting;

    [SerializeField]
    private int bossCuttingDamage = 5;


    private float timeActive;

    
    private Vector3 bufferVectorA;
    private Vector3 bufferVectorB;

    //single player light sabre power up variables
    public GameObject lightSabrePrefab;
    private GameObject lightSabreGameObject;

    private float sabreLength = 5.0f;

    private WaitForSeconds bossDamageCoolDown = new WaitForSeconds(0.5f);
    private bool bossTakesDamage = true;

    [SerializeField]
    private float lineSineFrequency = 20.0f;

    [SerializeField]
    private float colorChangeSpeed = 0.4f;

    [SerializeField]
    private float amplitudeChangeSpeed = 0.4f;

    [SerializeField]
    private float lineEndOffset = 0.0f;

    private float currentHeightOffset;

    private CuttingLineLogic cuttingLineLogic;
    private LineTweens lineTweens;

    private int[] lineSpeeds;
    private int initialSpeed = 200;
    

    private LineStatus[] lineStatus;

    #endregion

    #region properties

    public LineShaderUtility[] LineShaderUtilities
    {
        get { return lineShaderUtilities; }
        set { lineShaderUtilities = value; }
    }

    public GameObject[] Players
    {
        get { return players; }
        set { players = value; }
    }

    public bool[] IsChangingColor
    {
        get { return isChangingColor; }
        set { isChangingColor = value; }
    }

    public bool[] IsChangingAmplitude
    {
        get { return isChangingAmplitude; }
        set { isChangingAmplitude = value; }
    }


    public int[] ActiveColor
    {
        get { return activeColor; }
        set { activeColor = value; }
    }

    public Color[] Colors
    {
        get { return colors; }
    }

    public LineStatus[] Status
    {
        get { return lineStatus; }
        set { lineStatus = value; }
    }

    #endregion


    void Awake()
    {
        BasePlayer.PlayerDied += UpdatePlayerStatus;
        BasePlayer.PlayerSpawned += UpdatePlayerStatus;

        PolygonCoreLogic.PolyStarted += HideLines;
        PolygonCoreLogic.PolyEnded += ShowLines;
        PolygonCoreLogic.PolyExecuted += ShowLines;

        linesNeeded = new int[] { 0, 1, 3, 6 };
        firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
        secondVertex = new int[] { 1, 2, 0, 3, 3, 3 };
        playerMoved = new bool[4];
        playerHealing = new bool[4];
        nearbyPlayers = new bool[4, 4];

       

        playersInReach = new int[4];

        playerScripts = new BasePlayer[4];

        energyFillUp = new bool[4];       

        healingparticles = new GameObject[4];
        energyParticles = new ParticleSystem[4];
        healthParticles = new ParticleSystem[4];

        activateCutting = false;
        timeActive = -0.01f;
 
        bufferVectorA = new Vector3();
        bufferVectorB = new Vector3();

        currentHeightOffset = lineStartOffset;

    }

	// Use this for initialization
	void Start () {
        for (int i = 0; i < energyParticles.Length; i++)
        {
            healthParticles[i] = (Instantiate(health, Vector3.zero, Quaternion.Euler(-90, 0, 0)) as GameObject).GetComponent<ParticleSystem>();
            healthParticles[i].enableEmission = false;

            energyParticles[i] = (Instantiate(energy, Vector3.zero, Quaternion.Euler(-90, 0, 0)) as GameObject).GetComponent<ParticleSystem>();
            energyParticles[i].enableEmission = false;
        }


        lineTweens = gameObject.AddComponent<LineTweens>();
        lineTweens.LineSystem = this;

        cuttingLineLogic = gameObject.AddComponent<CuttingLineLogic>();
        cuttingLineLogic.LaserParticles = laserParticles;
        cuttingLineLogic.LighSabrePrefab = lightSabrePrefab;
        cuttingLineLogic.LineStartOffset = lineStartOffset;
        cuttingLineLogic.LineTweens = lineTweens;
        cuttingLineLogic.LineSystem = this;


        UpdatePlayerStatus();

    }
	
	// Update is called once per frame
	void Update () {

        if (players.Length >1)
        {
            UpdateConnectionType();            

            UpdatePlayerPosition();

            HandleHealthRecovery();

            HealOverTime();

            //HealLineUpdate();

            //UpdateLineSpeed();

            UpdateLines();          
        }

        if(players.Length == 1)
        {
            energyParticles[0].transform.position = players[0].transform.position;

            if (playerScripts[0].Energy!= playerScripts[0].MaxEnergy)
            {
                playerScripts[0].StopEnergyRefill = false;
                energyParticles[0].enableEmission = true;
            }
            else 
            {
                playerScripts[0].StopEnergyRefill = true;
                energyParticles[0].enableEmission = false;
            }           
        } 
	}

    private void ResetNearbyPlayers()
    {
        for(int i = 0; i < nearbyPlayers.GetLength(0); i++)
        {
            for(int j = 0; j< nearbyPlayers.GetLength(1); j++)
            {
                nearbyPlayers[i, j] = false;
            }
        }
    }

    public void ActivateCutting()
    {
       cuttingLineLogic.TimeActive = powerUpTime;
    }

    private void HideLines()
    {
        for(int i = 0; i < lineShaderUtilities.Length; i++)
        {
            LeanTween.value(gameObject, lineStartOffset, lineEndOffset, colorChangeSpeed)
            .setOnUpdate((float offset) => { currentHeightOffset = offset; })
            .setEase(LeanTweenType.pingPong);
        }
    }

    private void ShowLines()
    {
        for (int i = 0; i < lineShaderUtilities.Length; i++)
        {
            LeanTween.value(gameObject, lineEndOffset, lineStartOffset, colorChangeSpeed)
            .setOnUpdate((float offset) => { currentHeightOffset = offset; })
            .setEase(LeanTweenType.pingPong);
        }
    }

    void UpdateConnectionType()
    {
        for (int i = 0; i < lineShaderUtilities.Length; i++)
        {
            float distance = Vector3.Distance(players[firstVertex[i]].transform.position, players[secondVertex[i]].transform.position);

            if (cuttingLineLogic.TimeActive > 0.0f && !isChangingColor[i] && activeColor[i] != 2)
            {
                lineTweens.TweenColor(i, 2, false);
            }
            else if ((activeColor[i] == 0 || activeColor[i] == 2) && !isChangingColor[i] && distance < healDistance && cuttingLineLogic.TimeActive <= 0.0f)
            {
                lineTweens.TweenColor(i, 1, true);
            }

            else if ((activeColor[i] == 1 || activeColor[i] == 2) && !isChangingColor[i] && distance >= healDistance && cuttingLineLogic.TimeActive <= 0.0f)
            {
                lineTweens.TweenColor(i, 0, true);
            }
        }
    }

  
    void UpdatePlayerPosition()
    {
        for (int i = 0; i < players.Length; i++)
        {
            playerMoved[i] = false;
            energyParticles[i].transform.position = playerScripts[i].transform.position;
            healthParticles[i].transform.position = playerScripts[i].transform.position;

            if (playerScripts[i].IsMoving)
            {
                playerMoved[i] = true;
                playerHealing[i] = false;
            }
            else
            {
                playerMoved[i] = false;
            }
        }
    }


    void HandleHealthRecovery()
    {
        ResetNearbyPlayers();
        for (int i = 0; i < players.Length; i++)
        {
            playersInReach[i] = 0;
            for (int j = 0; j < players.Length; j++)
            {
                if (i != j)
                {
                    if (Vector3.Distance(players[i].transform.position, players[j].transform.position) <= healDistance)
                    {
                        playersInReach[i]++;
                        nearbyPlayers[i, j] = true;

                        if (!playerMoved[i])
                        {
                            playerHealing[i] = true;
                        }
                        else
                        {
                            playerHealing[i] = false;
                            healthParticles[i].enableEmission = false;

                        }
                        if (!playerMoved[j])
                        {
                            playerHealing[j] = true;
                        }
                        else
                        {
                            playerHealing[j] = false;
                            healthParticles[j].enableEmission = false;
                        }
                    }
                }
            }
        }
    }

    void DebugNearbyPlayers()
    {
        for(int i = 0; i < nearbyPlayers.GetLength(0); i++)
        {
            string tmp = "player "+i+" | ";
            for(int j = 0; j < nearbyPlayers.GetLength(1); j++)
            {
                tmp += nearbyPlayers[i, j] + " ";
            }
            tmp += "|";
            Debug.Log(tmp);
        }
    }


    private int FindLine(int playerA, int playerB)
    {
        for(int i = 0; i < linesNeeded[players.Length-1]; i++)
        {
            if((firstVertex[i] == playerA && secondVertex[i] == playerB) || (firstVertex[i] == playerB && secondVertex[i] == playerA))
            {
                Debug.Log("Returned line: " + i);
                return i;
            }
        }
        return -1;
    }


    private void HealLineUpdate()
    {
        for(int i = 0; i < players.Length; i++)
        {
            if (playerWasHealed[i])
            {
                for(int j = 0; j < nearbyPlayers.GetLength(1); j++)
                {
                    if (i!=j && nearbyPlayers[i, j])
                    {
                        int line = FindLine(i,j);
                        if (line != -1 && !isChangingAmplitude[line] && lineStatus[line] == LineStatus.normal)
                        {
                            lineStatus[line] = LineStatus.healAnimation;
                            isChangingAmplitude[line] = true;
                            lineTweens.TweenAmplitude(line, 0.4f);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < nearbyPlayers.GetLength(1); j++)
                {
                    if (i != j && nearbyPlayers[i, j])
                    {
                        int line = FindLine(i, j);
                        if (line != -1 && !isChangingAmplitude[line] && lineStatus[line] == LineStatus.heal)
                        {
                            lineStatus[line] = LineStatus.normalAnimation;
                            isChangingAmplitude[line] = true;
                            lineTweens.TweenAmplitude(line, 0.0f);
                        }
                    }
                }
            }
        }
    }


    private void UpdateLineSpeed()
    {
        for(int i = 0; i < linesNeeded[players.Length-1]; i++)
        {

            if(lineStatus[i] == LineStatus.heal)
            {
                int healthA = playerScripts[firstVertex[i]].Health;
                int healthB = playerScripts[secondVertex[i]].Health;
               

                if (healthA > healthB && lineSpeeds[i] > 0)
                {
                    lineSpeeds[i] *= -1;
                }
              
                lineShaderUtilities[i].speed = lineSpeeds[i];
            }

        }
    }

    void HealOverTime()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (playerHealing[i])
            {
                if (playerScripts[i].Health >0 &&  playerScripts[i].Health < playerScripts[i].MaxHealth && players[i].activeSelf)
                {
                    incremenetTimers[i] += Time.deltaTime* playersInReach[i] * healAmount;
                    playerWasHealed[i] = true;
                    if (incremenetTimers[i] >= 1.0f)
                    {
                        incremenetTimers[i] = 0.0f;
                        playerScripts[i].Health += 1;
                    }                      
                    healthParticles[i].enableEmission = true;
                }
                else if(playerScripts[i].Health <=0 || !players[i].activeSelf || playerScripts[i].Health == playerScripts[i].MaxHealth)
                {                    
                   healthParticles[i].enableEmission = false;
                    playerWasHealed[i] = false;
                }
            }
            else
            {                
                healthParticles[i].enableEmission = false;
                
            }

            if (playersInReach[i] > 0)
            {
                if (playerScripts[i].Energy <= 99)
                {
                    energyFillUp[i] = true;
                    playerScripts[i].StopEnergyRefill = false;
                    energyParticles[i].enableEmission = true;
                }
                else
                {
                    energyFillUp[i] = false;
                    playerScripts[i].StopEnergyRefill = true;
                    energyParticles[i].enableEmission = false;
                }
            }else
            {
                playerScripts[i].StopEnergyRefill = true;
                energyParticles[i].enableEmission = false;
            }
        }
    }



    private void HandleAmplitudeChange(int index, float endAmplitude)
    {
        for(int j =0; j < nearbyPlayers.GetLength(1); j++)
        {
            if (nearbyPlayers[index,j])
            {
                int line = FindLine(index, j);
                if (line != -1 && !isChangingAmplitude[line])
                {
                    lineTweens.TweenAmplitude(line, endAmplitude);
                }
            }
        }
    }


    void UpdateLines()
    {
        for (int i = 0; i < lineShaderUtilities.Length; i++)
        {
            lineShaderUtilities[i].startPosition =(new Vector3(players[firstVertex[i]].transform.position.x, players[firstVertex[i]].transform.position.y + currentHeightOffset, players[firstVertex[i]].transform.position.z));
            lineShaderUtilities[i].endPosition = ( new Vector3(players[secondVertex[i]].transform.position.x, players[secondVertex[i]].transform.position.y + currentHeightOffset, players[secondVertex[i]].transform.position.z));
        }

        for (int i = 0; i < players.Length; i++)
        {           
            if (playerHealing[i])
            {
                playerHealing[i] = false;
            }
        }
    }


    void UpdatePlayerStatus(BasePlayer basePlayer)
    {
        UpdatePlayerStatus();
    }


    void UpdatePlayerStatus()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        int playerCount = players.Length;

        if (playerCount < 4)
        {
            for (int i = playerCount; i < healingparticles.Length; i++)
            {
                if (healingparticles[i] != null)
                {
                    Destroy(healingparticles[i].gameObject);
                }
            }
        }


        for (int i = 0; i < energyParticles.Length; i++)
        {
            if (energyParticles[i] != null)
            {
                energyParticles[i].enableEmission = false;
            }            

            if(healthParticles[i] != null)
            {
                healthParticles[i].enableEmission = false;
            }
        }


        for (int i = 0; i < players.Length; i++){
            playerScripts[i] = players[i].GetComponent<BasePlayer>();
        }

        if (this != null)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
        }
        


        if (players.Length > 1)
        {
            lineShaderUtilities = new LineShaderUtility[linesNeeded[players.Length - 1]];
            isChangingColor = new bool[linesNeeded[players.Length - 1]];
            isChangingAmplitude = new bool[linesNeeded[players.Length - 1]];
            activeColor = new int[linesNeeded[players.Length - 1]];
            activeHealSineLine = new bool[linesNeeded[players.Length - 1]];
            lineStatus = new LineStatus[linesNeeded[players.Length - 1]];
            lineSpeeds = new int[linesNeeded[players.Length - 1]];

            incremenetTimers = new float[players.Length];
            playerWasHealed = new bool[players.Length];

            for (int i = 0; i < linesNeeded[players.Length - 1]; i++)
            {
                GameObject go = new GameObject("LineObject");
                LineRenderer line = go.AddComponent<LineRenderer>();
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                lineShaderUtilities[i] = go.AddComponent<LineShaderUtility>();
                lineShaderUtilities[i].lineMaterial = new Material(lineShaderMaterial);
                lineShaderUtilities[i].width = 0.25f;
                lineShaderUtilities[i].colorStrength = 1.5f;
                lineShaderUtilities[i].amplitude = 0.0f;
                lineShaderUtilities[i].smoothing = 0.23f;
                go.transform.parent = this.gameObject.transform;
                lineSpeeds[i] = initialSpeed;
            }  
        }         
    }


}
