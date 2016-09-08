using UnityEngine;
using System.Collections.Generic;

public delegate void PolygonEnemyDeathHandler(int bodyCount);

public class PolygonEnemyDetection : MonoBehaviour {

    #region variables
    private List<GameObject> detectedEnemies = new List<GameObject>();
    private BossEnemy[] bossDetected = new BossEnemy[1];
    private GameObject[] polygonParts;
    private GameObject[] playerGameObjects;
    private Mesh[] polygonMeshes;

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
        Debug.Log(System.DateTime.Now.ToString() + " Polygon explosion executed!");

        CameraManager.CameraReference.ShakeOnce();

        SoundManager.SoundManagerInstance.Play(polygonProperties.polyExplosion, Vector3.zero);
        new Event(Event.TYPE.superAbility).addPlayerCount().addWave().addLevel().addPos(polygonCoreLogic.MiddlePoint.x, polygonCoreLogic.MiddlePoint.z).addKills(detectedEnemies.Count).send();

        for (int i = 0; i < detectedEnemies.Count; i++)
        {
            
            detectedEnemies[i].GetComponent<BaseEnemy>().InstantKill(this);
            detectedEnemies[i].AddComponent<PolyExplosion>();
        }

        if (bossDetected[0] != null)
        {
            bossDetected[0].TakeDamage(polygonProperties.bossDamage[playerGameObjects.Length-1], this);
        }
      
        detectedEnemies.Clear();
        bossDetected[0] = null;
    }
    #endregion

    #region eventMethod
    private void OnPolygonEnemyDeaths()
    {       
        if (PolygonEnemyDeaths != null)
        { 
            PolygonEnemyDeaths(detectedEnemies.Count);
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
