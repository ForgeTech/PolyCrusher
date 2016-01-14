using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

// Poly event handler
public delegate void PolyExecuteEventHandler();
public delegate void PolyFailEventHandler();

public class PolygonSystem : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> enemies;

    [HideInInspector]
    public bool detonate;

    [Header("Particles")]
    public GameObject explosionParticle;
    public GameObject bigExplosionParticle;
    public GameObject playerStandingParticle;

    [Header("Sound files ")]
    public AudioClip polyLoading;
    public AudioClip polyExplosion;
    public AudioClip polyFail;    

    [Header("Materials for the polygon")]
    public Material[] mats;

    [Header("polygon setup")]
    public float heightOffset;
    public float polyStartHeight;
    public float polyStartSpeed;
    public float standStillRequiredTime;
    public float requiredPolyDistance;
    public float transitionCooldown;

    [Header("Damage the boss takes when the polygon hits")]
    public int bossDamage;


    private GameObject[] players;
    private bool loadingSoundPlaying;
    private bool declineSoundPlaying;

    private float polyLerpDistance;
    private Vector3 point;
    private Color[] flashColors;    

    private Vector3[] oldPlayerPosition;   
    private bool specialIsUsable;
    private int oldDonkey;
    private int donkey;  

    private MeshFilter flippedMeshFilter;
    private MeshFilter meshFilter;
    private SphereCollider sphere;

    private Vector3 middlePoint = new Vector3();
    private Mesh polygon;
    private Mesh triPolygon;
    private float[] angles;    

    private Vector3 intersectPosition;
    private float explosionCooldown;
    private bool polyIsStarting;
    private bool polyIsEnding;
    private bool polyIsFailing;
    private bool polyIsLoading;
    private bool screenFlash;
    private bool fadeOut;

    private GameObject poly;
    private GameObject triPoly;
    private GameObject screenFade;
    

    private Image whiteScreen;    
    private GameObject[] polyParts;
    private Mesh[] polys;
    private MeshFilter[] filters;
    private MeshRenderer[] renderers;
    private MeshCollider[] colliders;
    private BasePlayer[] playerScripts;
    private float[] polyOffsets;

    private float[] polyStartAnimLerpTimes;
    private float polyEndAnimLerpTime;
    private float polyFailAnimLerpTime;
    private float polyLoadingAnimLerpTime;
    private float screenFlashAnimTime;

    private bool polyFailTween;
    private bool polyFailTween2;

   
    private float currentCooldown;
    private bool[] polyTweens;

    private bool polyStart;
    private bool polyEnd;

    private int colliderFrameTime;
    private float currentStandStill;
    private int[] intersectedLines;

    private int[] linesNeeded;
    private int[] firstVertex;
    private int[] secondVertex;

    private Vector3[] cornerPoints;

    // Polygon events
    public static PolyExecuteEventHandler PolyExecuted;
    public static PolyFailEventHandler PolyFailed;

    void Awake()
    {
        BasePlayer.PlayerDied += UpdatePlayerStatus;
        BasePlayer.PlayerSpawned += UpdatePlayerStatus;
        LevelEndManager.levelExitEvent += ResetValues;

        screenFade = new GameObject("Canvas Container");
        Canvas canvas = screenFade.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        whiteScreen = screenFade.AddComponent<Image>();
        whiteScreen.color = Color.clear;
        screenFade.transform.SetParent(this.transform, false);
    }


    // Use this for initialization
    void Start()
    {
        linesNeeded = new int[] { 0, 1, 3, 6 };
        firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
        secondVertex = new int[] { 1, 2, 0, 3, 3, 3 };

        screenFlash = false;
        cornerPoints = new Vector3[4];
        enemies = new List<GameObject>();  
        polyParts = new GameObject[4];
        polys = new Mesh[4];
        filters = new MeshFilter[4];
        renderers = new MeshRenderer[4];
        colliders = new MeshCollider[4];
        polyOffsets = new float[4];
        polyStartAnimLerpTimes = new float[4];
        polyTweens = new bool[4];       

        flashColors = new Color[2];
        flashColors[0] = Color.clear;
        flashColors[1] = new Color(1, 1, 1, 0.7f);


        for (int i = 0; i < polyParts.Length; i++)
        {
            polyParts[i] = new GameObject();
            polyParts[i].transform.parent = gameObject.transform;
            polys[i] = new Mesh();
            filters[i] = polyParts[i].AddComponent<MeshFilter>();
            renderers[i] = polyParts[i].AddComponent<MeshRenderer>();
            renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[i].receiveShadows = false;
            renderers[i].material = mats[1];
            polyParts[i].layer = LayerMask.NameToLayer("ExplosionTriangle");
            polyOffsets[i] = polyStartHeight;
            colliders[i] = polyParts[i].AddComponent<MeshCollider>();
            colliders[i].sharedMesh = polys[i];
            colliders[i].convex = true;
            colliders[i].isTrigger = true;
            colliders[i].enabled = false;
            polyParts[i].AddComponent<TriangleCollision>();
        }       

        detonate = false;
        UpdatePlayerStatus();
    }

    /// <summary>
    /// this method detects enemies that are within a certain readius to the player(s) 
    /// </summary>
    private void DetectPlayerNearEnemies()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Collider[] colls = Physics.OverlapSphere(players[i].transform.position, 4.0f);
            foreach (Collider coll in colls)
            {
                if (coll.tag == "Enemy")
                {
                    if(coll.GetComponent<MonoBehaviour>() is BossEnemy)
                    {
                        coll.GetComponent<BossEnemy>().TakeDamage(bossDamage, null);
                    }
                    else
                    {
                        coll.gameObject.tag = "SentencedToDeath";
                        coll.GetComponent<BaseEnemy>().CanShoot = false;
                        coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
                        enemies.Add(coll.gameObject);
                    }                    
                }
            }
        }
        currentCooldown = 3.0f;
        ChainExplosion();       
    }

    /// <summary>
    /// every listed normal enemy is killed, a boss enemy takes damage 
    /// </summary>
    private void ChainExplosion()
    {        
        SoundManager.SoundManagerInstance.Play(polyExplosion, Vector3.zero);
        
        for (int i = 0; i<enemies.Count; i++)
        {            
            enemies[i].AddComponent<PolyExplosion>();
            enemies[i].GetComponent<BaseEnemy>().InstantKill();
        }  
        currentCooldown = 0.05f;


        if (enemies.Count == 0)
        {
            OnPolyFailed();
        }
        else if (enemies.Count >= 3)
        {
            OnPolyExecuted();
        }


        enemies = new List<GameObject>();    
        RestorePlayerPowers();
    }



    /// <summary>
    /// resets the health and energy values after using the polygon ability
    /// </summary>
    void RestorePlayerPowers()
    {
        for(int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<BasePlayer>().Health = 100;
            players[i].GetComponent<BasePlayer>().Energy = 50;
        }

        StartCoroutine("PlayerInvincibility");
    }


    void FixedUpdate()
    {
        if (detonate && currentCooldown <= 0.0f)
        {          
            colliderFrameTime--;

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
                colliders[i].sharedMesh = null;
                colliders[i].sharedMesh = polys[i];
            }

            if (colliderFrameTime <= 0)
            {
                detonate = false;
                colliderFrameTime = 2;

                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].sharedMesh = null;
                    colliders[i].enabled = false;
                }              
                DetectPlayerNearEnemies();
            }
        }
    }

    /// <summary>
    /// used to give the players 2 seconds of invincibility 
    /// </summary>
    IEnumerator PlayerInvincibility()
    {
        for(int i = 0; i < players.Length; i++)
        {
            playerScripts[i].Invincible = true;
        }

        yield return new WaitForSeconds(2.0f);

        for (int i = 0; i < players.Length; i++)
        {
            playerScripts[i].Invincible = false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (players.Length > 1)
        {

            GetAlignment(2);
            if (players.Length == 4)
            {
                if (!DonkeyInTheMiddle())
                {                    
                    donkey = -1;
                    HandleIntersection();
                }
            }

            UpdatePolyLerpDistance();
            UpdatePolyMaterial();

            int playerEnergyCheck = players.Length;

            for (int i = 0; i < players.Length; i++)
            {
                if (playerScripts[i].Energy == 100)
                {
                    playerEnergyCheck--;
                }
            }

            bool distanceReached = true;

            for (int i = 0; i < players.Length; i++)
            {
                for (int j = i+1; j < players.Length; j++)
                {
                    if (i != j && Vector3.Distance(players[i].transform.position, players[j].transform.position) < requiredPolyDistance)
                    {
                        distanceReached = false;
                    }
                }
            }

            if (playerEnergyCheck == 0 && distanceReached && polyStart && currentStandStill < standStillRequiredTime && currentCooldown <= 0.0f)
            {
                currentStandStill += Time.deltaTime;
                if (currentStandStill >= standStillRequiredTime && !detonate && currentCooldown <= 0.0f)
                {
                    currentStandStill = 0.0f;
                    screenFlash = true;
                    detonate = true;
                }
            }

            if (currentCooldown > 0.0f)
            {
                currentCooldown -= Time.deltaTime;
            }
            else
            {
                if (playerEnergyCheck == 0 && !polyStart && !polyIsStarting)
                {
                    polyIsStarting = true;
                    for(int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].material = mats[0];
                    }
                }

                else if (playerEnergyCheck == 0 && polyStart && currentStandStill > 0.0f && distanceReached)
                {
                    if (!loadingSoundPlaying)
                    {
                        loadingSoundPlaying = true;
                        SoundManager.SoundManagerInstance.Play(polyLoading, Vector3.zero);
                        StartCoroutine("SoundCoolDown");
                    }

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].material.Lerp(mats[0], mats[3], currentStandStill);

                    }

                }

                else if (playerEnergyCheck == 0 && polyStart && currentStandStill > 0.0f && !distanceReached)
                {
                    if (!declineSoundPlaying)
                    {
                        declineSoundPlaying = true;
                        SoundManager.SoundManagerInstance.Play(polyFail, Vector3.zero);
                        StartCoroutine("SoundDeclineCoolDown");
                    }
                    
                    currentStandStill = 0.0f;
                    currentCooldown = transitionCooldown;
                    polyIsFailing = true;
                }

                else if (playerEnergyCheck != 0 && polyStart && !polyIsEnding)
                {                    
                    polyIsEnding = true;
                }
            }



            if (polyIsStarting && !polyIsEnding && !polyIsFailing)
            {
                PolyStartAnimation();
            }
            if (polyIsEnding && !polyIsStarting && !polyIsFailing)
            {
                PolyEndAnimation();
            }
            if (polyIsFailing && !polyIsEnding && !polyIsStarting)
            {
                PolyFailAnimation();
            }           



            if (screenFlash)
            {
                if (!fadeOut && screenFlashAnimTime < 1.0f)
                {
                    screenFlashAnimTime += Time.deltaTime * 5.0f;
                    whiteScreen.color = Color.Lerp(flashColors[0], flashColors[1], screenFlashAnimTime);
                    if (screenFlashAnimTime >= 1.0f)
                    {
                        fadeOut = true;
                    }
                }

                if (fadeOut && screenFlashAnimTime > 0.0f)
                {
                    screenFlashAnimTime -= Time.deltaTime;
                    whiteScreen.color = Color.Lerp(flashColors[0], flashColors[1], screenFlashAnimTime);

                    if (screenFlashAnimTime <= 0.0f)
                    {
                        screenFlash = false;
                        fadeOut = false;
                        whiteScreen.color = Color.clear;
                        screenFlashAnimTime = 0.0f;
                        GamePad.SetVibration(0, 0, 0);
                    }
                }
            }           
            HandleSpecialPolygon();            
        }
        else
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = mats[1];
            }
        }    



        if (oldDonkey != donkey)
        {            
            oldDonkey = donkey;
        }
    }



    private void UpdatePolyLerpDistance()
    {
        float distance = 0.0f;
        bool firstSet = false;

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                float distanceNew = Vector3.Distance(players[i].transform.position, players[j].transform.position);

                if (!firstSet)
                {
                    firstSet = true;
                    distance = distanceNew;
                }
                else if (distance >= distanceNew)
                {
                    distance = distanceNew;
                }
            }
        }

        distance /= requiredPolyDistance ;
        polyLerpDistance = distance;
        
    }

    private void UpdatePolyMaterial()
    {   
        for (int i = 0; i < renderers.Length; i++)
        {        
            renderers[i].material.Lerp(mats[4], mats[0], polyLerpDistance);
        }
    }



   

    private IEnumerator SoundCoolDown()
    {
        yield return new WaitForSeconds(1.2f);
        loadingSoundPlaying = false;

    }

    private IEnumerator SoundDeclineCoolDown()
    {
        yield return new WaitForSeconds(1.2f);
        declineSoundPlaying = false;

    }



    private void PolyFailAnimation()
    {
        if (polyFailAnimLerpTime <= 1.0f)
        {
            polyFailAnimLerpTime += Time.deltaTime;
        }     

        for (int i = 0; i < renderers.Length; i++)
        {           
            renderers[i].material.Lerp(mats[4], mats[0], polyLerpDistance);
            renderers[i].material.Lerp(mats[3], renderers[i].material , polyFailAnimLerpTime * 2);
        }       

        if (polyFailAnimLerpTime > 1.0f)
        {           
            polyIsFailing = false;          
            polyFailAnimLerpTime = 0.0f;
            currentCooldown = transitionCooldown;
        }
    }


    private bool PlayersStandStill(int index)
    {
        if (index == -1)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (playerScripts[i].IsMoving)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return !playerScripts[index].IsMoving;
        }
    }






    private void PolyStartAnimation()
    {
        for (int i = 0; i < polyOffsets.Length; i++)
        {
            renderers[i].material.Lerp(mats[1], mats[0], polyLerpDistance);
            
            if (i == 0)
            {
                if (polyStartAnimLerpTimes[i] <= 0.0f && !polyTweens[i])
                {
                    polyTweens[i] = true;
                    Vector3 originalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    polyParts[i].transform.localScale = new Vector3(0.8f, 0.0f, 0.8f);

                    StartCoroutine(polyParts[i].transform.ScaleTo(originalScale, 0.7f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
                }

                if (polyStartAnimLerpTimes[i] <= 1.0f)
                {
                    polyStartAnimLerpTimes[i] += Time.deltaTime;
                    polyOffsets[i] = Mathf.Lerp(polyStartHeight, 0.0f, polyStartAnimLerpTimes[i] * polyStartSpeed);
                }
            }
            else
            {
                if (polyStartAnimLerpTimes[i - 1] >= 0.25f && polyStartAnimLerpTimes[i] <= 1.0f)
                {

                    if (polyStartAnimLerpTimes[i] <= 0.0f && !polyTweens[i])
                    {
                        polyTweens[i] = true;
                        Vector3 originalScale = new Vector3(1.0f, 1.0f, 1.0f);
                        polyParts[i].transform.localScale = new Vector3(0.8f, 0.0f, 0.8f);

                        StartCoroutine(polyParts[i].transform.ScaleTo(originalScale, 0.7f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
                    }
                    polyStartAnimLerpTimes[i] += Time.deltaTime;
                    polyOffsets[i] = Mathf.Lerp(polyStartHeight, 0.0f, polyStartAnimLerpTimes[i] * polyStartSpeed);
                }
            }
        }

        if (polyStartAnimLerpTimes[polyStartAnimLerpTimes.Length - 1] > 1.0f)
        {
            for (int i = 0; i < polyStartAnimLerpTimes.Length; i++)
            {
                polyStartAnimLerpTimes[i] = 0.0f;
                polyTweens[i] = false;
            }
            polyStart = true;
            polyIsStarting = false;
        }
    }


    private void PolyEndAnimation()
    {
        if (polyEndAnimLerpTime <= 
            1.0f)
        {
            polyEndAnimLerpTime += Time.deltaTime;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.Lerp(renderers[i].material, mats[1], polyEndAnimLerpTime);
        }

        if (polyEndAnimLerpTime >= 1.0f)
        {
            for (int i = 0; i < polyOffsets.Length; i++)
            {
                polyOffsets[i] = polyStartHeight;
                renderers[i].material = mats[1];

            }
            polyStart = false;
            polyEndAnimLerpTime = 0.0f;
            polyIsEnding = false;

            currentCooldown = transitionCooldown;
        }
    }



    /// <summary>
    /// signn function used for point in tri calculation
    /// </summary>
    /// <param name="p1">Point1</param>
    /// <param name="p2">Point2</param>
    /// <param name="p3">Point3</param>
    /// <returns></returns>
    float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }


    /// <summary>
    /// checks if a point is in a triangle
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    private bool IsPointInTri(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        bool b1, b2, b3;

        b1 = Sign(pt, v1, v2) < 0.0f;
        b2 = Sign(pt, v2, v3) < 0.0f;
        b3 = Sign(pt, v3, v1) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    /// <summary>
    /// if the playercount is 4, then this method determines which one of them is the middle one
    /// </summary>
    /// <returns></returns>
    private bool DonkeyInTheMiddle()
    {
        for (int i = 0; i < players.Length; i++)
        {

            if (IsPointInTri(players[i % 4].transform.position, players[(i + 1) % 4].transform.position, players[(i + 2) % 4].transform.position, players[(i + 3) % 4].transform.position))
            {
                intersectedLines[0] = -1;
                intersectedLines[1] = -1;
                donkey = i;
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// used to determine, which line connections cross in the middle (if they do)
    /// </summary>
    void HandleIntersection()
    {
        intersectedLines[0] = -1;
        intersectedLines[1] = -1;       
        {            
            for (int i = 0; i < linesNeeded[players.Length - 1]; i++)
            {
                float x1 = players[firstVertex[i]].transform.position.x;
                float y1 = players[firstVertex[i]].transform.position.z;

                float x2 = players[secondVertex[i]].transform.position.x;
                float y2 = players[secondVertex[i]].transform.position.z;

                for (int j = 0; j < linesNeeded[players.Length - 1]; j++)
                {

                    if (i != j)
                    {
                        float x3 = players[firstVertex[j]].transform.position.x;
                        float y3 = players[firstVertex[j]].transform.position.z;

                        float x4 = players[secondVertex[j]].transform.position.x;
                        float y4 = players[secondVertex[j]].transform.position.z;

                        float A1 = y2 - y1;
                        float B1 = x1 - x2;
                        float C1 = A1 * x1 + B1 * y1;

                        float A2 = y4 - y3;
                        float B2 = x3 - x4;
                        float C2 = A2 * x3 + B2 * y3;

                        float delta = (A1 * B2) - (A2 * B1);
                        if (delta != 0)
                        {
                            float interX = (B2 * C1 - B1 * C2) / delta;
                            float interY = (A1 * C2 - A2 * C1) / delta;

                            intersectPosition = new Vector3(interX, heightOffset, interY);

                            float newX1 = x1;
                            float newY1 = y1;
                            float newX2 = x2;
                            float newY2 = y2;

                            float newX3 = x3;
                            float newY3 = y3;
                            float newX4 = x4;
                            float newY4 = y4;

                            if (x1 > x2)
                            {
                                newX1 = x2;
                                newX2 = x1;
                            }

                            if (y1 > y2)
                            {
                                newY1 = y2;
                                newY2 = y1;
                            }

                            if (x3 > x4)
                            {
                                newX3 = x4;
                                newX4 = x3;
                            }

                            if (y3 > y4)
                            {
                                newY3 = y4;
                                newY4 = y3;
                            }


                            if (newX1 + 0.01 < interX && newX2 - 0.010 > interX && newY1 + 0.010 < interY && newY2 - 0.010 > interY && newX3 + 0.010 < interX && newX4 - 0.010 > interX && newY3 + 0.010 < interY && newY4 - 0.010 > interY)
                            {
                                intersectedLines[0] = i;
                                intersectedLines[1] = j;
                                return;
                            }
                        }
                    }
                }
            }
            intersectedLines[0] = -1;
            intersectedLines[1] = -1;
        }
    }


    /// <summary>
    /// provides the current rotation angle for the players according to a certain middle point
    /// important for the correct polygon mesh construction
    /// </summary>
    /// <param name="type"></param>
    private void GetAlignment(int type)
    {
        Vector3 middle = new Vector3();

        for(int i = 0; i < players.Length; i++)
        {
            if (i != donkey)
            {
                middle += players[i].transform.position;
            }
        }
        if (donkey != -1)
        {
            middle /= (players.Length-1);
        }
        else
        {
            middle /= players.Length;           
        }

        for (int i = 0; i < players.Length; i++)
        {
            Vector3 tmp = new Vector3();
            Vector3 tmp2 = new Vector3();
            if (type == 1)
            {
                tmp = (players[i].transform.position - middlePoint);
                tmp2 = (Vector3.zero - middlePoint);
            }
            else if (type == 0)
            {
                tmp = (players[i].transform.position - players[donkey].transform.position);
                tmp2 = (Vector3.zero - players[donkey].transform.position);
            }
            else if(type == 2)
            {               
                Vector3 tmp3 = middle * 100.0f;
                tmp3 = new Vector3(Mathf.Round(tmp3.x), Mathf.Round(tmp3.y), Mathf.Round(tmp3.z));
                tmp3 /= 100.0f;                
                tmp3.y += 5.0f;
           
                tmp = (players[i].transform.position -tmp3)*100.0f;
                tmp = new Vector3(Mathf.Round(tmp.x), Mathf.Round(tmp.y), Mathf.Round(tmp.z));
                tmp /= 100.0f;

                tmp2 = (tmp3);
            }

            float newAngle =Mathf.Rad2Deg*(Mathf.Atan2(tmp.x * tmp2.x, tmp.z * tmp2.z));        
            angles[i] = newAngle;
        }

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                if (i != j && angles[i] < angles[j])
                {
                    GameObject go = players[i];
                    players[i] = players[j];
                    players[j] = go;

                    float tmp = angles[i];
                    angles[i] = angles[j];
                    angles[j] = tmp;
                }
            }
        }
    }


    void HandleSpecialPolygon()
    {
        if (players.Length == 4)
        {
            if (players.Length == 4 && donkey == -1)
            {
                middlePoint = intersectPosition;


                Vector3[] vbot = new Vector3[players.Length + 1];
                Vector3[] vtop = new Vector3[players.Length + 1];
                for (int i = 0; i < players.Length; i++)
                {

                    vbot[i] = players[i].transform.position;
                    vbot[i].y += heightOffset;

                    vtop[i] = players[i].transform.position;
                    vtop[i].y += heightOffset + 0.1f;

                }
                vbot[players.Length] = middlePoint;

                vtop[players.Length] = middlePoint;
                vtop[players.Length].y += 0.1f;

                for (int i = 0; i < polyParts.Length; i++)
                {
                    polys[i].vertices = new Vector3[] { new Vector3(vbot[i].x, vbot[i].y + polyOffsets[i], vbot[i].z), new Vector3(vbot[(i + 1) % 4].x, vbot[(i + 1) % 4].y + polyOffsets[i], vbot[(i + 1) % 4].z), new Vector3(vbot[4].x, vbot[4].y + polyOffsets[i], vbot[4].z), new Vector3(vtop[i].x, vtop[i].y + polyOffsets[i], vtop[i].z), new Vector3(vtop[(i + 1) % 4].x, vtop[(i + 1) % 4].y + polyOffsets[i], vtop[(i + 1) % 4].z), new Vector3(vtop[4].x, vtop[4].y + polyOffsets[i], vtop[4].z) };
                    polys[i].normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    polys[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(1, 0.5f), new Vector2(1, 1) };
                    polys[i].triangles = new int[] { 0, 1, 2, 2, 1, 0, 0, 1, 4, 4, 1, 0, 0, 3, 4, 4, 3, 0, 0, 2, 5, 5, 2, 0, 1, 4, 5, 5, 4, 1, 3, 4, 5, 5, 4, 3, 0, 3, 5, 5, 3, 0, 1, 2, 5, 5, 2, 1 };
                    polys[i].Optimize();
                    polys[i].RecalculateBounds();
                    filters[i].sharedMesh = polys[i];
                    cornerPoints[i] = vbot[i];
                    
                }


            }
            else if (donkey != -1)
            {
                middlePoint = players[donkey].transform.position;
                middlePoint.y += heightOffset;

                Vector3[] vbot = new Vector3[players.Length];
                Vector3[] vtop = new Vector3[players.Length];
                int currentPos = 0;

                for (int i = 0; i < players.Length; i++)
                {
                    if (i != donkey)
                    {
                        vbot[currentPos] = players[i].transform.position;
                        vbot[currentPos].y += heightOffset;


                        vtop[currentPos] = players[i].transform.position;
                        vtop[currentPos].y += heightOffset + 0.1f;

                        cornerPoints[currentPos++] = vbot[i];

                    }                  

                }
                vbot[players.Length - 1] = middlePoint;
                vtop[players.Length - 1] = middlePoint;
                vtop[players.Length - 1].y += 0.1f;


                for (int i = 0; i < polyParts.Length - 1; i++)
                {
                    polys[i].vertices = new Vector3[] { new Vector3(vbot[i].x, vbot[i].y + polyOffsets[i], vbot[i].z), new Vector3(vbot[(i + 1) % 3].x, vbot[(i + 1) % 3].y + polyOffsets[i], vbot[(i + 1) % 3].z), new Vector3(vbot[3].x, vbot[3].y + polyOffsets[i], vbot[3].z), new Vector3(vtop[i].x, vtop[i].y + polyOffsets[i], vtop[i].z), new Vector3(vtop[(i + 1) % 3].x, vtop[(i + 1) % 3].y + polyOffsets[i], vtop[(i + 1) % 3].z), new Vector3(vtop[3].x, vtop[3].y + polyOffsets[i], vtop[3].z) };
                    polys[i].normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    polys[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(1, 0.5f), new Vector2(1, 1) };
                    polys[i].triangles = new int[] { 0, 1, 2, 2, 1, 0, 0, 1, 4, 4, 1, 0, 0, 3, 4, 4, 3, 0, 0, 2, 5, 5, 2, 0, 1, 4, 5, 5, 4, 1, 3, 4, 5, 5, 4, 3, 0, 3, 5, 5, 3, 0, 1, 2, 5, 5, 2, 1 };
                    polys[i].Optimize();
                    polys[i].RecalculateBounds();
                    filters[i].sharedMesh = polys[i];
                   
                }

                polys[players.Length - 1].Clear();

            }
        }

        if (players.Length == 3)
        {
            Vector3[] vbot = new Vector3[players.Length + 1];
            Vector3[] vtop = new Vector3[players.Length + 1];

            middlePoint = new Vector3();

            for (int i = 0; i < players.Length; i++)
            {
                vbot[i] = players[i].transform.position;
                vbot[i].y += heightOffset;

                vtop[i] = players[i].transform.position;
                vtop[i].y += heightOffset + 0.1f;
                
                middlePoint += vbot[i];               
            }

            middlePoint /= 3;


            vbot[players.Length] = middlePoint;

            vtop[players.Length] = middlePoint;
            vtop[players.Length].y +=  0.1f;

            for (int i = 0; i < players.Length; i++)
            {
                polys[i].vertices = new Vector3[] { new Vector3(vbot[i].x, vbot[i].y + polyOffsets[i], vbot[i].z), new Vector3(vbot[(i + 1) % 3].x, vbot[(i + 1) % 3].y + polyOffsets[i], vbot[(i + 1) % 3].z), new Vector3(vbot[3].x, vbot[3].y + polyOffsets[i], vbot[3].z), new Vector3(vtop[i].x, vtop[i].y + polyOffsets[i], vtop[i].z), new Vector3(vtop[(i + 1) % 3].x, vtop[(i + 1) % 3].y + polyOffsets[i], vtop[(i + 1) % 3].z), new Vector3(vtop[3].x, vtop[3].y + polyOffsets[i], vtop[3].z) };
                polys[i].normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                polys[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(1, 0.5f), new Vector2(1, 1) };
                polys[i].triangles = new int[] { 0, 1, 2, 2, 1, 0, 0, 1, 4, 4, 1, 0, 0, 3, 4, 4, 3, 0, 0, 2, 5, 5, 2, 0, 1, 4, 5, 5, 4, 1, 3, 4, 5, 5, 4, 3, 0, 3, 5, 5, 3, 0, 1, 2, 5, 5, 2, 1 };
                polys[i].Optimize();
                polys[i].RecalculateBounds();
                filters[i].sharedMesh = polys[i];
                cornerPoints[i] = vbot[i];

            }

            polys[players.Length].Clear();







        }

        if (players.Length == 2)
        {
            Vector3 direction = players[0].transform.position - players[1].transform.position;

            Vector3 pos = (players[0].transform.position + players[1].transform.position) * 0.5f;

            middlePoint = pos;

            Vector3 pointA = pos + (Quaternion.Euler(0, -90, 0) * direction * 0.5f);

            Vector3 pointB = pos + (Quaternion.Euler(0, 90, 0) * direction * 0.5f);

            Vector3[] vbot = new Vector3[players.Length + 3];
            Vector3[] vtop = new Vector3[players.Length + 3];






            vbot[0] = players[0].transform.position;
            vbot[0].y += heightOffset;

            vtop[0] = players[0].transform.position;
            vtop[0].y += heightOffset+0.1f;


            vbot[1] = pointA;
            vbot[1].y += heightOffset;

            vtop[1] = pointA;
            vtop[1].y += heightOffset+0.1f;



            vbot[2] = players[1].transform.position;
            vbot[2].y += heightOffset;

            vtop[2] = players[1].transform.position;
            vtop[2].y += heightOffset+0.1f;




            vbot[3] = pointB;
            vbot[3].y += heightOffset;

            vtop[3] = pointB;
            vtop[3].y += heightOffset+0.1f;


            vbot[4] = pos;
            vbot[4].y += heightOffset;

            vtop[4] = pos;
            vtop[4].y += heightOffset+0.1f;



            for (int i = 0; i < cornerPoints.Length; i++)
            {
                cornerPoints[i] = vbot[i];
            }

            for (int i = 0; i < polyParts.Length; i++)
            {
                polys[i].vertices = new Vector3[] { new Vector3(vbot[i].x, vbot[i].y + polyOffsets[i], vbot[i].z), new Vector3(vbot[(i + 1) % 4].x, vbot[(i + 1) % 4].y + polyOffsets[i], vbot[(i + 1) % 4].z), new Vector3(vbot[4].x, vbot[4].y + polyOffsets[i], vbot[4].z), new Vector3(vtop[i].x, vtop[i].y + polyOffsets[i], vtop[i].z), new Vector3(vtop[(i + 1) % 4].x, vtop[(i + 1) % 4].y + polyOffsets[i], vtop[(i + 1) % 4].z), new Vector3(vtop[4].x, vtop[4].y + polyOffsets[i], vtop[4].z) };
                polys[i].normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                polys[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(1, 0.5f), new Vector2(1, 1) };
                polys[i].triangles = new int[] { 0, 1, 2, 2, 1, 0, 0, 1, 4, 4, 1, 0, 0, 3, 4, 4, 3, 0, 0, 2, 5, 5, 2, 0, 1, 4, 5, 5, 4, 1, 3, 4, 5, 5, 4, 3, 0, 3, 5, 5, 3, 0, 1, 2, 5, 5, 2, 1 };
                polys[i].Optimize();
                polys[i].RecalculateBounds();
                filters[i].sharedMesh = polys[i];
            }
        }        
    }

  

    /// <summary>
    /// calls the normal UpdatePlayerStatus(), the basePlayer variable is not important for this update
    /// </summary>
    /// <param name="basePlayer"></param>
    void UpdatePlayerStatus(BasePlayer basePlayer)
    {
        UpdatePlayerStatus();
    }


    /// <summary>
    /// method is called whenever a player is spawned or killed to upate alle the necessary properties for the polygon
    /// </summary>
    void UpdatePlayerStatus()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        playerScripts = new BasePlayer[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            playerScripts[i] = players[i].GetComponent<BasePlayer>();
            

        }

      
        oldPlayerPosition = new Vector3[players.Length];              
        angles = new float[players.Length];
       
        intersectedLines = new int[2];
        intersectedLines[0] = -1;
        intersectedLines[1] = -1;

        for (int i = 0; i < players.Length; i++)
        {
            oldPlayerPosition[i] = players[i].transform.position;
        }
    }

    /// <summary>
    /// Event method for the "PolyExecuted" event.
    /// </summary>
    protected void OnPolyExecuted()
    {
        if (PolyExecuted != null)
            PolyExecuted();
    }

    /// <summary>
    /// Event method for the "PolyFailed" event.
    /// </summary>
    protected void OnPolyFailed()
    {
        if (PolyFailed != null)
            PolyFailed();
    }

    /// <summary>
    /// Resets values.
    /// </summary>
    protected void ResetValues()
    {
        PolyFailed = null;
        PolyExecuted = null;
    }
}