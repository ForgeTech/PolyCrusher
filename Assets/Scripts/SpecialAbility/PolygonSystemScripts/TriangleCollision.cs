using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public delegate void DetectionFinishedHandler();

public class TriangleCollision : MonoBehaviour {

    #region variables
    private List<GameObject> detectedEnemies;
    private BossEnemy bossEnemy;
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private static int collidersFinished = 0;
    private static int currentColliderCount = 0;
    private WaitForFixedUpdate fixedWait;

    public static event DetectionFinishedHandler DetectionDone;
    #endregion

    #region properties
    public List<GameObject> DetectedEnemies
    {
        set { detectedEnemies = value; }
    } 

    public BossEnemy BossEnemy
    {
        set { bossEnemy = value; }
    }
    #endregion

    #region methods
    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
        PolygonCoreLogic.PolyExecuted += HandleEnemyDetection;
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        fixedWait = new WaitForFixedUpdate();
        currentColliderCount++;
        Debug.Log("colliders: " + currentColliderCount);
    }

    private void HandleEnemyDetection()
    {
        StartCoroutine(DetectEnemies());
    }

    private IEnumerator DetectEnemies()
    {
        meshCollider.enabled = true;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;
        
        yield return fixedWait;

        meshCollider.sharedMesh = null;
        meshCollider.enabled = true;

        yield return fixedWait;
        OnDetectionDone();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Enemy")
        {
            if (coll.GetComponent<MonoBehaviour>() is BossEnemy)
            {
                if (bossEnemy == null)
                {
                    bossEnemy = coll.GetComponent<BossEnemy>();
                }
            }
            else
            {
                coll.tag = "SentencedToDeath";
                coll.GetComponent<BaseEnemy>().CanShoot = false;
                coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
                detectedEnemies.Add(coll.gameObject);
            }
        }
    }

    #region eventMethod
    public void OnDetectionDone()
    {
        collidersFinished++;
        if (collidersFinished >= currentColliderCount)
        {
             if (DetectionDone != null)
            {
                collidersFinished = 0;
                DetectionDone();
            }
        }
    }
    #endregion

    #region eventReset
    private void ResetValues()
    {
        DetectionDone = null;
    }
    #endregion

    #endregion



}
