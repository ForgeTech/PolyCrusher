using UnityEngine;
using System.Collections;


public delegate void PolygonStartedHandler();
public delegate void PolygonEndedHandler();
public delegate void PolygonExecutedHandler();
public delegate void PolygonFailedHandler();


public class PolygonCoreLogic : MonoBehaviour {

    #region variables
    public static event PolygonStartedHandler PolyStarted;
    public static event PolygonEndedHandler PolyEnded;
    public static event PolygonExecutedHandler PolyExecuted;
    public static event PolygonFailedHandler PolyFailed;

    private GameObject[] polyParts;
    private Mesh[] polys;
    private MeshFilter[] filters;
    private MeshRenderer[] renderers;
    private float[] polygonPartHeightOffsets;

    private BasePlayer[] playerScripts;
    private GameObject[] playerGameObjects;

    private PolygonSystem polygonSystem;
    private PolygonProperties polygonProperties;
    private PolygonUtil polygonUtil;
    private PolygonMeshBuilder polygonMeshBuilder;

    private AudioSource audioPlayer;

    private int donkey = -1;

    private float polyLerpDistance = 0.0f;

    private Vector3 middlePoint = new Vector3();
    private bool polygonIsStarting = false;

    private bool polygonIsActive = false;

    private bool polygonIsInactive = true;

    private bool polygonIsEnding = false;

    private bool polygonIsDeCharging = false;

    private bool cuttingIsActive = false;

    private float currentPolyTriggerTime = 0.0f;

    private float currentAlpha = 0.0f;

    #endregion

    #region properties
    public PolygonProperties PolygonProperties
    {
        set { polygonProperties = value;
            InitializePolygonPartMeshes();
            UpdatePlayerStatus();
        }
    }

    public PolygonSystem PolygonSystem
    {
        set { polygonSystem = value; }
    }

    public PolygonUtil PolygonUtil
    {
        set { polygonUtil = value; }
    }

    public PolygonMeshBuilder PolygonMeshBuilder
    {
        set { polygonMeshBuilder = value; }
    }

    public float[] PolygonPartHeightOffsets
    {
        get { return polygonPartHeightOffsets; }
        set { polygonPartHeightOffsets = value; }
    }

    public GameObject[] PlayerGameObjects
    {
        get { return playerGameObjects; }
    }

    public Vector3 MiddlePoint
    {
        get { return middlePoint; }
    }

    public GameObject[] PolygonParts
    {
        get { return polyParts; }
    }

    public Mesh[] PolygonMeshes
    {
        get { return polys; }
    }
    #endregion

    #region methods

    #region initialization

    void Awake()
    {
        BasePlayer.PlayerSpawned += UpdatePlayerStatus;
        BasePlayer.PlayerDied += UpdatePlayerStatus;
        LevelEndManager.levelExitEvent += ResetValues;
        PolygonTweens.PolygonStartAnimationFinished += SetPolygonActive;
        PolygonTweens.PolygonEndAnimationFinished += SetPolygonInactive;

        CuttingLineLogic.CuttingActive += OnCuttingActivated;
        CuttingLineLogic.CuttingInactive += OnCuttingDeactivated;

        polygonMeshBuilder = gameObject.AddComponent<PolygonMeshBuilder>();


        audioPlayer = gameObject.AddComponent<AudioSource>();
        audioPlayer.loop = false;
        audioPlayer.Stop();
    }

    private void InitializePolygonPartMeshes()
    {
        if (polygonProperties != null)
        {
            polyParts = new GameObject[4];
            polys = new Mesh[4];
            filters = new MeshFilter[4];
            renderers = new MeshRenderer[4];
            polygonPartHeightOffsets = new float[4];

            for (int i = 0; i < polyParts.Length; i++)
            {
                polyParts[i] = new GameObject();
                polyParts[i].transform.parent = gameObject.transform;
                polys[i] = new Mesh();
                filters[i] = polyParts[i].AddComponent<MeshFilter>();
                filters[i].sharedMesh = polys[i];
                renderers[i] = polyParts[i].AddComponent<MeshRenderer>();
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].material = polygonProperties.polygonMaterials[0];
                polyParts[i].layer = LayerMask.NameToLayer("ExplosionTriangle");
                polygonPartHeightOffsets[i] = polygonProperties.polyStartHeight;
            }
        }
        else
        {
            Debug.LogError("Polygon properties object is null!");
        }
    }

    #endregion

    #region donkeyInTheMiddle
    /// <summary>
    /// sign function used for point in tri calculation
    /// </summary>
    /// <param name="p1">Point1</param>
    /// <param name="p2">Point2</param>
    /// <param name="p3">Point3</param>
    /// <returns></returns>
    private float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
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
    private int DonkeyInTheMiddle()
    {
        if (playerGameObjects.Length == 4)
        {
            for (int i = 0; i < playerGameObjects.Length; i++)
            {
                if (IsPointInTri(playerGameObjects[i % 4].transform.position, playerGameObjects[(i + 1) % 4].transform.position, playerGameObjects[(i + 2) % 4].transform.position, playerGameObjects[(i + 3) % 4].transform.position))
                {
                    return i;
                }
            }
        }
        return -1;
    }
    #endregion

    #region polygonUpdateCycle
    private void Update()
    {
        if (playerGameObjects.Length > 1)
        {
            //updating information
            polyLerpDistance = polygonUtil.CalculatePolygonLerpDistance(playerGameObjects, polygonProperties.requiredPolyDistance);
            donkey = DonkeyInTheMiddle();
            middlePoint = CalculateNewMiddlePoint();
            playerGameObjects = polygonUtil.AllignPlayers(playerGameObjects, donkey);

            //core logic
            if (polygonUtil.CheckPlayerEnergyLevels(playerScripts) && !polygonIsStarting && !polygonIsActive && polygonIsInactive && !cuttingIsActive)
            {
                polygonIsInactive = false;
                polygonIsStarting = true;
                OnPolyStarted();               
            }

            if (polygonIsActive)
            {

                if (!polygonUtil.CheckPlayerEnergyLevels(playerScripts))
                {
                    polygonIsActive = false;
                    OnPolyEnded();
                }
                else
                {
                    if(polygonUtil.CheckPlayerDistances(playerGameObjects, polygonProperties.requiredPolyDistance))
                    {
                        if(currentPolyTriggerTime == 0)
                        {
                            audioPlayer.Stop();
                            audioPlayer.clip = polygonProperties.polyLoading;
                            audioPlayer.Play();
                        }

                        currentPolyTriggerTime += Time.deltaTime;
                        if (currentPolyTriggerTime >= polygonProperties.requiredTriggerTime)
                        {
                            OnPolyExecuted();
                        }
                    }
                    else
                    {
                        if (!polygonIsDeCharging)
                        {
                            polygonIsDeCharging = true;
                            LeanTween.value(gameObject, currentPolyTriggerTime, 0.0f, 0.5f)
                               .setOnUpdate((float time) => { currentPolyTriggerTime = time; })
                               .setEase(LeanTweenType.easeInOutSine)
                               .setOnComplete(() => { polygonIsDeCharging = false; });
                        }
                    }
                }
                

                



            }

            //updating components
            UpdatePolygonMaterialColor();
            UpdatePolygonMaterialBrightness();
            UpdatePolygonMaterialAlpha();
            polygonMeshBuilder.UpdateMeshInformations(playerGameObjects, middlePoint, filters, polys, donkey, polygonPartHeightOffsets);
        }
    }
    #endregion

    #region calculateMiddlePoint
    private Vector3 CalculateNewMiddlePoint()
    {
        middlePoint.Set(0, 0, 0);
        if (donkey == -1)
        {
            for(int i = 0; i < playerGameObjects.Length; i++)
            {
                middlePoint += playerGameObjects[i].transform.position + polygonProperties.heightOffset;
            }
            middlePoint /= playerGameObjects.Length;
        }
        else
        {
            middlePoint = playerGameObjects[donkey].transform.position + polygonProperties.heightOffset;
        }
        return middlePoint;
    }

    #endregion

    #region polyMaterialUpdate

    private void UpdatePolygonMaterialColor()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.Lerp(polygonProperties.polygonMaterials[2], polygonProperties.polygonMaterials[0], polyLerpDistance);
        }
    }
    #endregion

    #region adjustment
    private void AdjustPlayerValues()
    {
        for (int i = 0; i < playerScripts.Length; i++)
        {
            playerScripts[i].Health = playerScripts[i].MaxHealth;
            playerScripts[i].Energy = 50;
        }
    }

    private void SetPolygonActive()
    {
        polygonIsStarting = false;
        polygonIsActive = true;
    }

    private void SetPolygonInactive()
    {
        polygonIsEnding = false;
        polygonIsInactive = true;
    }

    private void UpdatePolygonMaterialBrightness()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.Lerp(renderers[i].material, polygonProperties.polygonMaterials[3], currentPolyTriggerTime);
        }
    }

    private void UpdatePolygonMaterialAlpha()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.Lerp(renderers[i].material, polygonProperties.polygonMaterials[1], currentAlpha);
        }
    }

    private void OnCuttingActivated()
    {
        cuttingIsActive = true;
    }

    private void OnCuttingDeactivated()
    {
        cuttingIsActive = false;
    }
    #endregion

    #region updatePlayerInformation

    /// <summary>
    /// calls the normal UpdatePlayerStatus(), the basePlayer variable is not important for this update
    /// </summary>
    /// <param name="basePlayer"></param>
    private void UpdatePlayerStatus(BasePlayer basePlayer)
    {
        UpdatePlayerStatus();
    }


    /// <summary>
    /// method is called whenever a player is spawned or killed to update all the necessary properties for the polygon
    /// </summary>
    private void UpdatePlayerStatus()
    {
        playerGameObjects = GameObject.FindGameObjectsWithTag("Player");
        playerScripts = new BasePlayer[playerGameObjects.Length];

        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            playerScripts[i] = playerGameObjects[i].GetComponent<BasePlayer>();
        }

        if (playerScripts.Length <= 1 && !polygonIsInactive)
        {
            OnPolyEnded();
            polygonMeshBuilder.ClearMeshes(filters, polys);
        }
    }

   
    #endregion

    #region eventMethods

    //when all players have full energy the polygon starts
    private void OnPolyStarted()
    {
        currentAlpha = 0.0f;
        if (PolyStarted != null)
        {
            PolyStarted();
        }
    }

    //when the polygon is running,and a player loses energy, the polygon ends
    private void OnPolyEnded()
    {
        if (PolyEnded != null)
        {
            PolyEnded();
        }
    }

    //when the players use the ability, the polygon is executed
    private void OnPolyExecuted()
    {
        currentPolyTriggerTime = 0.0f;
        polygonIsActive = false;
        polygonIsEnding = true;
        AdjustPlayerValues();

        audioPlayer.Stop();
        audioPlayer.clip = polygonProperties.polyExplosion;
        audioPlayer.Play();

        LeanTween.value(gameObject, currentAlpha, 1.0f, 0.5f)
                              .setOnUpdate((float value) => { currentAlpha = value; })
                              .setEase(LeanTweenType.easeInOutSine)
                              .setOnComplete(()=> { SetPolygonInactive();
                                  for(int i = 0; i < polygonPartHeightOffsets.Length; i++)
                                  {
                                      polygonPartHeightOffsets[i] = polygonProperties.polyStartHeight;
                                  }
                              });  

        if (PolyExecuted != null)
        {
            PolyExecuted();
        }
    }
    #endregion

    #region reset
    /// <summary>
    /// Resets values.
    /// </summary>
    private void ResetValues()
    {
        PolyStarted = null;
        PolyEnded = null;
        PolyFailed = null;
        PolyExecuted = null;
    }
    #endregion

    #endregion
}
