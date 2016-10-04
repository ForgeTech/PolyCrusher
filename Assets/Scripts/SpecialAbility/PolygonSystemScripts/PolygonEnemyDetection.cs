using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public delegate void PolygonEnemyDeathHandler(int bodyCount);

public class PolygonEnemyDetection : MonoBehaviour {

    #region variables
    private List<GameObject> detectedEnemies = new List<GameObject>();
    private BossEnemy[] bossDetected = new BossEnemy[1];
    private GameObject[] polygonParts;
    private GameObject[] playerGameObjects;
    private Mesh[] polygonMeshes;
    private BaseEnemy toDestroy;
    private EnemyEnum enemyIdentifier;

    public static event PolygonEnemyDeathHandler PolygonEnemyDeaths;

    private PolygonCoreLogic polygonCoreLogic;
    private PolygonProperties polygonProperties;
    
    #endregion

    #region properties
    public PolygonCoreLogic PolygonCoreLogic
    {
        set { polygonCoreLogic = value;
            AddCollisionComponents();
        }
    }

    public PolygonProperties PolygonProperties
    {
        set { polygonProperties = value; }
    }
    #endregion

    #region methods

    #region initialization
    void Start () {
        detectedEnemies.Clear();        
        TriangleCollision.DetectionDone += HandlePolyExecution;
        LevelEndManager.levelExitEvent += Reset;
    }

    private void AddCollisionComponents()
    {
        playerGameObjects = polygonCoreLogic.PlayerGameObjects;
        polygonParts = polygonCoreLogic.PolygonParts;
        polygonMeshes = polygonCoreLogic.PolygonMeshes;

        TriangleCollision triangleCollision;
        MeshCollider meshCollider;
        for (int i = 0; i < polygonParts.Length; i++)
        {
            meshCollider = polygonParts[i].AddComponent<MeshCollider>();
            meshCollider.sharedMesh = polygonMeshes[i];
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
            meshCollider.enabled = false;
            triangleCollision = polygonParts[i].AddComponent<TriangleCollision>();
            triangleCollision.DetectedEnemies = detectedEnemies;
            triangleCollision.BossEnemy = bossDetected;
        }
    }
    #endregion

    #region polyExecution
    private void HandlePolyExecution()
    {
        DetectNearEnemies();
        OnPolygonEnemyDeaths();
        ChainExplosion();
    }
    #endregion

    #region nearEnemyDetection
    /// <summary>
    /// this method detects enemies that are within a certain readius to the player(s)
    /// </summary>
    private void DetectNearEnemies()
    {
        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            Collider[] colls = Physics.OverlapSphere(playerGameObjects[i].transform.position, 2.0f);
            foreach (Collider coll in colls)
            {
                if (coll.tag == "Enemy")
                {
                    if (coll.GetComponent<MonoBehaviour>() is BossEnemy)
                    {
                        if (bossDetected[0] == null)
                        {
                            bossDetected[0] = coll.GetComponent<BossEnemy>();
                        }
                    }
                    else
                    {
                        coll.gameObject.tag = "SentencedToDeath";
                        coll.GetComponent<BaseEnemy>().CanShoot = false;
                        coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
                        detectedEnemies.Add(coll.gameObject);
                    }
                }
            }
        }
    }
    #endregion

    #region explosion
    /// <summary>
    /// every listed normal enemy is killed, a boss enemy takes damage 
    /// </summary>
    private void ChainExplosion()
    {
        CameraManager.CameraReference.ShakeOnce();

        SoundManager.SoundManagerInstance.Play(polygonProperties.polyExplosion, Vector3.zero, AudioGroup.Effects);
        new Event(Event.TYPE.superAbility).addPlayerCount().addWave().addLevel().addPos(polygonCoreLogic.MiddlePoint.x, polygonCoreLogic.MiddlePoint.z).addKills(detectedEnemies.Count).send();

        if (bossDetected[0] != null)
        {
            bossDetected[0].TakeDamage(polygonProperties.bossDamage[playerGameObjects.Length-1], this);
            bossDetected[0].gameObject.AddComponent<BossPolyHitExplosion>();

        }
        bossDetected[0] = null;

        StartCoroutine(ExplodeOverTime());
    }
    #endregion

    #region explosionOverTime
    private IEnumerator ExplodeOverTime()
    {
        for (int i = 0; i < detectedEnemies.Count; i++)
        {
            if (detectedEnemies[i] != null)
            {
                toDestroy = detectedEnemies[i].GetComponent<BaseEnemy>();
                enemyIdentifier = toDestroy.EnemyIdentifier;
                toDestroy.PolyKill(this);
                if (enemyIdentifier == EnemyEnum.Coyote)
                {
                    detectedEnemies[i].AddComponent<BigScalePolyExplosion>();
                }
                else if (enemyIdentifier == EnemyEnum.ChewingGum)
                {
                    detectedEnemies[i].AddComponent<SmallPolyExplosion>();
                }
                else
                {
                    detectedEnemies[i].AddComponent<NormalPolyExplosion>();
                }
                yield return null;
            }
        }
        detectedEnemies.Clear();
    }
    #endregion

    #region eventMethod
    private void OnPolygonEnemyDeaths()
    {       
        if (PolygonEnemyDeaths != null)
        {
            if (bossDetected[0] != null)
            {
                PolygonEnemyDeaths(detectedEnemies.Count + 1);
            }   
            else
            {
                PolygonEnemyDeaths(detectedEnemies.Count);
            }
        }      
    }
    #endregion

    #region reset
    private void Reset()
    {
        PolygonEnemyDeaths = null;
    }
    #endregion

    #endregion
}
