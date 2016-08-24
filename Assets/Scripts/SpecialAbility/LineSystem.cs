using UnityEngine;
using System.Collections;

public class LineSystem : MonoBehaviour {

    public float heightOffset;
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


  
   
    public Material[] mats;
    public GameObject health;
    public GameObject energy;
    public GameObject laserParticles;


    private ParticleSystem[] energyParticles;
    private ParticleSystem[] healthParticles;


    private bool isChangingHeal;
    private bool isChangingNormal;



    private GameObject[] players;
    private GameObject[] healingparticles;


    private  bool[] healAnimation;
    private bool[] normalAnimation;
    private bool[] cuttingAnimation;

    private Vector3[] oldPlayerPosition;

    private float[] incremenetTimers;  

    private bool[] playerMoved;
    private bool[] playerHealing;
    private int[] playersInReach;   


    private BasePlayer[] playerScripts;
    private LineRenderer[] lineRenderer;
    private int[] linesNeeded;
    private int[] firstVertex;
    private int[] secondVertex;
    private float[] lerpTimes;
   
    private bool[] energyFillUp;

    private bool sabreAnimationFinished;

    private int[] activeMaterial;
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

    private Vector3 sabreStartPoint;
    private Vector3 sabreEndPoint;

    private float sabreLength = 5.0f;
    private LineShaderUtility sabreLineShader;


    void Awake()
    {
        BasePlayer.PlayerDied += UpdatePlayerStatus;
        BasePlayer.PlayerSpawned += UpdatePlayerStatus;

        linesNeeded = new int[] { 0, 1, 3, 6 };
        firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
        secondVertex = new int[] { 1, 2, 0, 3, 3, 3 };
        playerMoved = new bool[4];
        playerHealing = new bool[4];

        playersInReach = new int[4];

        playerScripts = new BasePlayer[4];
        lerpTimes = new float[4];

        energyFillUp = new bool[4];       

        healingparticles = new GameObject[4];
        energyParticles = new ParticleSystem[4];
        healthParticles = new ParticleSystem[4];

        healAnimation = new bool[6];
        normalAnimation = new bool[6];
        cuttingAnimation = new bool[6];

        activateCutting = false;
        timeActive = -0.01f;
 

        bufferVectorA = new Vector3();
        bufferVectorB = new Vector3();

        sabreStartPoint = new Vector3();
        sabreEndPoint = new Vector3();

        sabreLineShader = GetComponent<LineShaderUtility>();

       
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
        UpdatePlayerStatus();
    }
	
	// Update is called once per frame
	void Update () {

        if (activateCutting)
        {
            activateCutting = false;
            timeActive = powerUpTime;
        }

        if (timeActive >= 0.0f)
        {
            timeActive -= Time.deltaTime;
            if (players.Length > 1)
            {
                CuttingLinesPowerUp();
                if (timeActive <= 0.0f)
                {
                    for (int i = 0; i < lineRenderer.Length; i++)
                    {
                        lineRenderer[i].material = mats[activeMaterial[i]];
                    }
                }
            }
            if(players.Length == 1)
            {
                PrepareLightSabre();
            }
        }

       


        if (players.Length >1)
        {
            UpdateConnectionType();            

            UpdatePlayerPosition();

           

            HandleHealthRecovery();

            HealOverTime();

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

    public void ActivateCutting()
    {
        if (timeActive <= 0.0f)
        {
            activateCutting = true;
        }
    }
      
    private void CuttingLinesPowerUp()
    {    
        for(int i = 0; i < lineRenderer.Length; i++)
        {                         
            RaycastHit[] hits;
            bufferVectorA = players[firstVertex[i]].transform.position;
            bufferVectorA.y = heightOffset;

            bufferVectorB = players[secondVertex[i]].transform.position;
            bufferVectorB.y = heightOffset;
          
            lineRenderer[i].SetWidth(healLineWidth, healLineWidth);

            hits = Physics.RaycastAll(new Ray(bufferVectorB, Vector3.Normalize(bufferVectorA - bufferVectorB)), Vector3.Distance(bufferVectorB, bufferVectorA), (1 << 9));           
           
            foreach (RaycastHit hit in hits)
            {              
                if (hit.transform.GetComponent<MonoBehaviour>())
                {
                    MonoBehaviour gotHit = hit.transform.GetComponent<MonoBehaviour>();
                    if (gotHit is BaseEnemy)
                    {
                        BaseEnemy enemy = hit.transform.GetComponent<BaseEnemy>();
                        if (gotHit is BossEnemy)
                        {
                            Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation), 2);
                            enemy.TakeDamage(bossCuttingDamage, this);
                        }
                        else
                        {
                            enemy.InstantKill(this);
                            enemy.gameObject.AddComponent<CutUpMesh>();
                            Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation),2);
                        }
                    }
                }
            }
        }  
    }


    private void PrepareLightSabre()
    {
        if (lightSabreGameObject == null)
        {
            lightSabreGameObject = Instantiate(lightSabrePrefab, Vector3.zero, Quaternion.identity) as GameObject;
            lightSabreGameObject.transform.parent = playerScripts[0].transform;
            lightSabreGameObject.transform.position = Vector3.zero;
            sabreLineShader = lightSabreGameObject.GetComponent<LineShaderUtility>();
            sabreLineShader.useWorldSpace = false;
        }
      
    }

    void UpdateConnectionType()
    {
        for (int i = 0; i < lineRenderer.Length; i++)
        {
            float distance = Vector3.Distance(players[firstVertex[i]].transform.position, players[secondVertex[i]].transform.position);

            if (timeActive > 0.0f && !cuttingAnimation[i] && activeMaterial[i] != 3)
            {
                cuttingAnimation[i] = true;

            }
            else if ((activeMaterial[i] == 0 || activeMaterial[i]==3 )&& !normalAnimation[i] && !healAnimation[i] && !cuttingAnimation[i] && distance < healDistance && timeActive<=0.0f)
            {
                healAnimation[i] = true;
            }

            else if ((activeMaterial[i] == 1 || activeMaterial[i] == 3) && !normalAnimation[i] && !healAnimation[i] && !cuttingAnimation[i] && distance >= healDistance && timeActive <= 0.0f)
            {
                normalAnimation[i] = true;               
            }

            if (cuttingAnimation[i])
            {
                if (lerpTimes[i] < 1.0f)
                {
                    lerpTimes[i] += lerpSpeed * Time.deltaTime;
                    lineRenderer[i].material.Lerp(mats[activeMaterial[i]], mats[3], lerpTimes[i]);
                }
                else
                {
                    lineRenderer[i].material = mats[3];
                    activeMaterial[i] = 3;
                    lerpTimes[i] = 0.0f;
                    cuttingAnimation[i] = false;
                }

            }
            else if (healAnimation[i])
            {
                if (lerpTimes[i] < 1.0f)
                {
                    lerpTimes[i] += lerpSpeed * Time.deltaTime;
                    lineRenderer[i].material.Lerp(mats[activeMaterial[i]], mats[1], lerpTimes[i]);
                }
                else
                {
                    lineRenderer[i].material = mats[1];
                    activeMaterial[i] = 1;
                    lerpTimes[i] = 0.0f;
                    healAnimation[i] = false;
                }
            }else if (normalAnimation[i])
            {
                if (lerpTimes[i] < 1.0f)
                {
                    lerpTimes[i] += lerpSpeed * Time.deltaTime;
                    lineRenderer[i].material.Lerp(mats[activeMaterial[i]], mats[0], lerpTimes[i]);
                }
                else
                {

                    lineRenderer[i].material = mats[0];
                    activeMaterial[i] = 0;
                    lerpTimes[i] = 0.0f;
                    normalAnimation[i] = false;
                }
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
            }else
            {
                playerMoved[i] = false;
            }
        }
    }


    void HandleHealthRecovery()
    {
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
                            healthParticles[i].enableEmission = false;
                        }
                    }
                }
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


    void UpdateLines()
    {
        for (int i = 0; i < lineRenderer.Length; i++)
        {
            lineRenderer[i].SetWidth(normalLineWidth, normalLineWidth);
            lineRenderer[i].SetPosition(0, new Vector3(players[firstVertex[i]].transform.position.x, players[firstVertex[i]].transform.position.y + heightOffset, players[firstVertex[i]].transform.position.z));
            lineRenderer[i].SetPosition(1, new Vector3(players[secondVertex[i]].transform.position.x, players[secondVertex[i]].transform.position.y + heightOffset, players[secondVertex[i]].transform.position.z));
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
            oldPlayerPosition = new Vector3[players.Length];

            oldPlayerPosition = new Vector3[players.Length];
            lineRenderer = new LineRenderer[linesNeeded[players.Length - 1]];
            lerpTimes = new float[linesNeeded[players.Length - 1]];
            activeMaterial = new int[linesNeeded[players.Length - 1]];

            for (int i = 0; i < players.Length; i++)
            {
                oldPlayerPosition[i] = players[i].transform.position;
            }
            
            incremenetTimers = new float[players.Length];

            for (int i = 0; i < linesNeeded[players.Length - 1]; i++)
            {
                GameObject go = new GameObject();
                lineRenderer[i] = go.AddComponent<LineRenderer>();
                lineRenderer[i].SetVertexCount(2);
                lineRenderer[i].sharedMaterial = mats[0];
                lineRenderer[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer[i].receiveShadows = false;

                go.transform.parent = this.gameObject.transform;
            }  
        }         
    }
}
